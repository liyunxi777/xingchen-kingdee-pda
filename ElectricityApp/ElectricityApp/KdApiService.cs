using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.Script.Serialization;
using LrGetToken;

namespace ElectricityApp
{
    /// <summary>
    /// 金蝶 API 查询服务（Smobiler 手机端复用）。
    /// 凭证从 app.config 的 appSettings 读取；app-token 进程内缓存，满 20 小时自动续期。
    /// 底层取数完全复用 LrGetToken 的 GetAppSecretB / GetToken / GetDatumX，不重造签名逻辑。
    /// 注意：本文件刻意不用 Newtonsoft.Json.Linq 类型——Smobiler.Plugins.dll 内嵌了一份 internal 的
    /// Newtonsoft.Json，会与包引用冲突导致 CS0122（JToken 不可访问）。改用 .NET 自带的 JavaScriptSerializer。
    /// </summary>
    public class KdApiService
    {
        private static readonly object _tokenLock = new object();
        private static string _appToken = "";
        private static DateTime _tokenIssuedAt = DateTime.MinValue;
        private static readonly TimeSpan TOKEN_REFRESH_BEFORE = TimeSpan.FromHours(20);

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _appKey;
        private readonly string _outerInstanceId;

        public KdApiService()
        {
            _clientId = Setting("Kd.ClientId");
            _clientSecret = Setting("Kd.ClientSecret");
            _appKey = Setting("Kd.AppKey");
            _outerInstanceId = Setting("Kd.OuterInstanceId");
        }

        private static string Setting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            return value ?? "";
        }

        private static JavaScriptSerializer NewSerializer()
        {
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            return ser;
        }

        /// <summary>安全取字典字段，键不存在返回 null。</summary>
        private static object GetVal(Dictionary<string, object> dict, string key)
        {
            if (dict == null) return null;
            object v;
            return dict.TryGetValue(key, out v) ? v : null;
        }

        /// <summary>值转显示文本；大整数避免被转成科学计数法。</summary>
        private static string ToStr(object v)
        {
            if (v == null) return "";
            if (v is string) return (string)v;
            if (v is double)
            {
                double d = (double)v;
                if (d == Math.Floor(d) && Math.Abs(d) >= 1e15) return d.ToString("0");
            }
            return v.ToString();
        }

        /// <summary>把 JSON 解析成字典根节点，失败返回 null。</summary>
        private static Dictionary<string, object> ParseRoot(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return NewSerializer().DeserializeObject(json) as Dictionary<string, object>; }
            catch { return null; }
        }

        /// <summary>
        /// 扫码查询骨架：依次按物料编码(number)、条码(barcode)精确查，均未命中再按 search 模糊查。
        /// 返回原始 JSON 字符串，失败返回空串。
        /// 注意：number 与 barcode 都要试——手机扫出的可能是物料编码，也可能是条码。
        /// </summary>
        public string QueryByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            EnsureToken();
            if (string.IsNullOrEmpty(_appToken)) return "";

            string trimmed = code.Trim();
            // 1) 物料编码精确查
            string result = Query("jdy/v2/bd/material", "number", trimmed);
            if (HasRows(result)) return result;
            // 2) 条码字段精确查（扫一维码/条码时命中）
            result = Query("jdy/v2/bd/material", "barcode", trimmed);
            if (HasRows(result)) return result;
            // 3) 模糊查询兜底
            return Query("jdy/v2/bd/material", "search", trimmed);
        }

        /// <summary>工单查询（扫码码值是工单号时使用，保留备用）。</summary>
        public string QueryWorkOrder(string billNo)
        {
            if (string.IsNullOrWhiteSpace(billNo)) return "";
            EnsureToken();
            if (string.IsNullOrEmpty(_appToken)) return "";
            return Query("jdy/v2/pm/mo_taskbill", "bill_no", billNo.Trim());
        }

        private string Query(string path, string key, string value)
        {
            var ps = new List<LrParam>();
            ps.Add(Param("page", "1"));
            ps.Add(Param("page_size", "20"));
            ps.Add(Param(key, value));
            return new LrGetMtrlDatum().GetDatumX(_clientId, _clientSecret, _appToken, ps, path);
        }

        private static LrParam Param(string key, string value)
        {
            return new LrParam { pKey = key, pValue = value };
        }

        /// <summary>
        /// 取数据行：兼容金蝶三种返回结构——data 直接是数组、data 是含 rows 的对象、data 是单条对象。
        /// 与主项目 FrmTest.cs 的解析逻辑一致。
        /// </summary>
        private static object[] GetRows(string json, out int total)
        {
            total = 0;
            Dictionary<string, object> root = ParseRoot(json);
            if (root == null) return null;
            object dataObj = GetVal(root, "data");
            if (dataObj == null) return null;

            // 结构一：data 直接是数组
            object[] arr = dataObj as object[];
            if (arr != null) return arr;

            // 结构二/三：data 是对象
            var dataDict = dataObj as Dictionary<string, object>;
            if (dataDict == null) return null;

            object totalObj = GetVal(dataDict, "total");
            if (totalObj != null)
            {
                int t;
                if (int.TryParse(ToStr(totalObj), out t)) total = t;
            }

            object[] rows = GetVal(dataDict, "rows") as object[];
            if (rows != null) return rows;

            // 结构三：data 本身就是单条对象，包成一行
            return new object[] { dataDict };
        }

        private static bool HasRows(string json)
        {
            int total;
            object[] data = GetRows(json, out total);
            return data != null && data.Length > 0;
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
                    ToStr(GetVal(row, "spec")),
                    ToStr(GetVal(row, "model")),
                    ToStr(GetVal(row, "measure_unit_name")),
                    ToStr(GetVal(row, "barcode")),
                    ""  // qty：物料档案无数量字段
                );
            }
            return dt;
        }

        /// <summary>确保 app-token 存在且未到续期时间；线程安全。</summary>
        private void EnsureToken()
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_appToken)
                    && DateTime.Now - _tokenIssuedAt < TOKEN_REFRESH_BEFORE) return;
                _appToken = FetchToken();
                _tokenIssuedAt = DateTime.Now;
            }
        }

        private string FetchToken()
        {
            try
            {
                var cls = new clsGetToken();
                string secretResp = cls.GetAppSecretB(_clientId, _clientSecret, _appKey, _outerInstanceId);
                Dictionary<string, object> jo = ParseRoot(secretResp);
                if (jo == null) return "";

                string appSecret = null;
                object[] data = GetVal(jo, "data") as object[];
                if (data != null)
                {
                    foreach (object o in data)
                    {
                        var item = o as Dictionary<string, object>;
                        if (item != null && ToStr(GetVal(item, "appKey")) == _appKey)
                        {
                            appSecret = ToStr(GetVal(item, "appSecret"));
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(appSecret)) return "";

                string tokenResp = cls.GetToken(_clientId, _clientSecret, _appKey, appSecret);
                Dictionary<string, object> to = ParseRoot(tokenResp);
                if (to == null) return "";
                var td = GetVal(to, "data") as Dictionary<string, object>;
                if (td == null) return "";
                return ToStr(GetVal(td, "app-token"));
            }
            catch
            {
                return "";
            }
        }

        /// <summary>第 4 步：扫码上报。待确认单据类型与 JSON 样例后实现。</summary>
        public string SubmitBill(string billJson)
        {
            throw new NotImplementedException();
        }
    }
}
