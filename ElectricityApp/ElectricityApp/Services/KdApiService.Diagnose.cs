using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ElectricityApp
{
    /// <summary>诊断与展示部分：DataTable 构建、字段兜底、可用编码/字段诊断文本。</summary>
    public partial class KdApiService
    {
        private static string Snippet(string json)
        {
            if (string.IsNullOrEmpty(json)) return "(空)";
            return json.Length <= 300 ? json : json.Substring(0, 300) + "...";
        }

        /// <summary>从物料列表 JSON 里提取前 max 条"编码=名称"，供诊断时显示当前账套可用编码。</summary>
        private static string DescribeRows(string json, int max)
        {
            int total;
            object[] rows = GetRows(json, out total);
            if (rows == null || rows.Length == 0) return "(物料列表为空或接口未返回数据)";
            var sb = new StringBuilder();
            int n = Math.Min(max, rows.Length);
            for (int i = 0; i < n; i++)
            {
                var row = rows[i] as Dictionary<string, object>;
                if (row == null) continue;
                if (sb.Length > 0) sb.Append("；");
                sb.Append(ToStr(GetVal(row, "number"))).Append("(")
                  .Append(ToStr(GetVal(row, "name"))).Append(")");
            }
            return sb.Length == 0 ? "(无编码字段)" : sb.ToString();
        }

        /// <summary>把接口 JSON 转成手机端易读文本：前 10 条 number / name / id。</summary>
        public string FormatForDisplay(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "查询失败：接口无返回（检查 token 或网络）";
            int total;
            object[] data = GetRows(json, out total);
            if (data == null || data.Length == 0) return "未查到数据";

            var sb = new StringBuilder();
            int shown = total > 0 ? total : data.Length;
            sb.Append("共 ").Append(shown).Append(" 条：\r\n");
            int n = Math.Min(10, data.Length);
            for (int i = 0; i < n; i++)
            {
                var row = data[i] as Dictionary<string, object>;
                sb.Append(i + 1).Append(". ")
                  .Append(ToStr(GetVal(row, "number"))).Append(" ")
                  .Append(ToStr(GetVal(row, "name"))).Append(" [")
                  .Append(ToStr(GetVal(row, "id"))).Append("]\r\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 把接口 JSON 转成手机表格用的 DataTable。
        /// 表头：编码 / 名称 / 规格 / 型号 / 单位 / 条码 / 数量。
        /// 其中"数量"物料主数据不提供，留空（数量属于库存/单据，需另接接口）。
        /// 字段名沿用金蝶原始字段：number/name/spec/model/measure_unit_name/barcode，与 FrmTest.cs 的 MaterialFieldMap 一致。
        /// </summary>
        public DataTable BuildResultTable(string json)
        {
            var dt = new DataTable();
            dt.Columns.Add("number", typeof(string));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("spec", typeof(string));
            dt.Columns.Add("model", typeof(string));
            dt.Columns.Add("measure_unit_name", typeof(string));
            dt.Columns.Add("barcode", typeof(string));
            dt.Columns.Add("qty", typeof(string));

            int total;
            object[] data = GetRows(json, out total);
            if (data == null) return dt;

            foreach (object o in data)
            {
                var row = o as Dictionary<string, object>;
                if (row == null) continue;
                dt.Rows.Add(
                    ToStr(GetVal(row, "number")),
                    ToStr(GetVal(row, "name")),
                    FieldOr(row, "spec", "specification", "model"),  // 规格：spec→specification→model(model 常为 1*4 这类规格值)
                    ToStr(GetVal(row, "model")),
                    FieldOr(row, "measure_unit_name", "primary_unit_name", GetPrimaryUnit(row)),  // 单位：直取→拍平列→units 解析
                    ToStr(GetVal(row, "barcode")),
                    FieldOr(row, "qty", "stock", "quantity")      // 数量：若接口偶有返回则用，否则空
                );
            }
            return dt;
        }

        /// <summary>依次尝试多个字段名，返回第一个非空值；都没有则返回空串。</summary>
        private static string FieldOr(Dictionary<string, object> row, params string[] keys)
        {
            foreach (string k in keys)
            {
                if (k == null) continue;
                string v = ToStr(GetVal(row, k));
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return "";
        }

        /// <summary>
        /// 从 units 字段解析主单位名称——与主程序 FrmTest.cs:433-459 逻辑一致。
        /// 精斗云物料的单位不在 measure_unit_name（该字段常为空），而是藏在 units：
        /// 一段字符串化 JSON 数组，其中 unit_type=1 的元素其 unit_name 即主单位。
        /// units 可能是字符串，也可能已被反序列化成 object[]，两种都兼容。
        /// </summary>
        private static string GetPrimaryUnit(Dictionary<string, object> row)
        {
            object unitsObj = GetVal(row, "units");
            if (unitsObj == null) return "";
            object[] arr = unitsObj as object[];
            if (arr == null)
            {
                string s = ToStr(unitsObj);
                if (string.IsNullOrWhiteSpace(s)) return "";
                try { arr = NewSerializer().DeserializeObject(s) as object[]; }
                catch { return ""; }
            }
            if (arr == null) return "";
            foreach (object o in arr)
            {
                var u = o as Dictionary<string, object>;
                if (u == null) continue;
                if (ToStr(GetVal(u, "unit_type")) == "1")
                    return ToStr(GetVal(u, "unit_name"));
            }
            return "";
        }

        /// <summary>
        /// 诊断用：列出首条物料在金蝶原始返回里的关键字段及值（空字段标"(空)"），
        /// 用于确认"规格/单位/数量为空"到底是金蝶没返回，还是代码取错字段名。
        /// </summary>
        public string DescribeFirstRowFields(string json)
        {
            int total;
            object[] data = GetRows(json, out total);
            if (data == null || data.Length == 0) return "";
            var row = data[0] as Dictionary<string, object>;
            if (row == null) return "";
            string[] keys = { "spec", "specification", "model", "measure_unit_name" };
            var sb = new StringBuilder();
            foreach (string k in keys)
            {
                string v = ToStr(GetVal(row, k));
                if (v.Length > 12) v = v.Substring(0, 12) + "...";
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(k).Append("=").Append(string.IsNullOrEmpty(v) ? "空" : v);
            }
            // units 解析结果单独显示：这是精斗云真正的单位来源
            string unit = GetPrimaryUnit(row);
            sb.Append(" units主单位=").Append(string.IsNullOrEmpty(unit) ? "空" : unit);
            return sb.ToString();
        }
    }
}
