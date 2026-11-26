using System;
using System.Collections.Generic;
using LrGetToken;

namespace ElectricityApp
{
    /// <summary>查询部分：物料列表翻页、编码/条码匹配、服务端 search 兜底、工单查询。</summary>
    public partial class KdApiService
    {
        /// <summary>
        /// 扫码查询：拉物料列表后本地按编码/条码匹配。
        /// 关键修正：金蝶 jdy/v2/bd/material 对 number/barcode 服务端过滤参数支持不稳定，
        /// 主程序 FrmTest.cs:364-368 从不传这些参数，而是用 page/page_size/show_units 拉列表后本地匹配。
        /// 这里沿用同一方式，保证主程序能查到的物料手机端也能查到。
        /// 返回 data.rows 结构的 JSON 供 BuildResultTable 解析；查不到返回空串。
        /// 成功与否不再靠"返回字符串是否为空"判断，而是显式写入
        /// LastSucceeded / LastErrorCode / LastErrorMessage，由 CheckResponse 基于业务状态判定。
        /// </summary>
        public string QueryByCode(string code)
        {
            LastDiagnostic = "";
            LastSucceeded = false;
            LastErrorCode = "";
            LastErrorMessage = "";

            if (string.IsNullOrWhiteSpace(code))
            {
                LastErrorCode = "EMPTY_INPUT";
                LastErrorMessage = "编码为空";
                return "";
            }

            EnsureToken();
            if (string.IsNullOrEmpty(_appToken))
            {
                LastErrorCode = "NO_TOKEN";
                LastErrorMessage = "token 获取失败（检查 app.config 凭证）";
                LastDiagnostic = LastErrorMessage;
                return "";
            }

            string trimmed = code.Trim();
            const int MAX_PAGE = 10;   // 最多拉 10 页 × 100 条 = 1000 个物料
            string firstJson = "";
            string interfaceErrCode = "", interfaceErrMsg = "";   // 接口层失败原因，与"查无此码"严格区分

            for (int page = 1; page <= MAX_PAGE; page++)
            {
                string pageJson = QueryMaterialPage(page);

                // 判断依据是数据有效性（IsSuccess/ErrorCode/业务码/rows），不是"字符串是否为空/是不是 JSON"
                string ec, em;
                if (!CheckResponse(pageJson, out ec, out em))
                {
                    interfaceErrCode = ec;
                    interfaceErrMsg = em;
                    break;   // 接口确实失败，停止翻页
                }

                if (page == 1) firstJson = pageJson;

                int total;
                object[] rows = GetRows(pageJson, out total);
                if (rows == null || rows.Length == 0) break;   // 没有更多数据

                // 1) number / barcode 精确匹配
                var exact = MatchRows(rows, trimmed, false);
                if (exact.Count > 0)
                {
                    LastSucceeded = true;
                    return WrapRows(exact);
                }

                // 2) number / name 模糊匹配（兜底）
                var fuzzy = MatchRows(rows, trimmed, true);
                if (fuzzy.Count > 0)
                {
                    LastSucceeded = true;
                    return WrapRows(fuzzy);
                }

                // 3) 容错匹配：去掉易误输入的字母 V 后再精确匹配（如 MV00001 -> M00001）。
                //    只在"唯一命中"时采用，避免查 A 却填成 B 的串料风险。
                var tolerant = MatchRowsIgnoringV(rows, trimmed);
                if (tolerant.Count == 1)
                {
                    LastSucceeded = true;
                    return WrapRows(tolerant);
                }
            }

            // 最后兜底：让金蝶服务端用 search 过滤一次（仅当列表接口本身没报错）
            if (string.IsNullOrEmpty(interfaceErrCode))
            {
                string searchJson = Query("jdy/v2/bd/material", "search", trimmed);
                string sec, sem;
                if (CheckResponse(searchJson, out sec, out sem))
                {
                    int dummy;
                    object[] srows = GetRows(searchJson, out dummy);
                    if (srows != null && srows.Length > 0)
                    {
                        LastSucceeded = true;
                        return searchJson;
                    }
                }
                else
                {
                    interfaceErrCode = sec;
                    interfaceErrMsg = sem;
                }
            }

            // 接口层确实失败：把错误码和原因暴露给界面（判断依据是数据有效性，不是字符串）
            if (!string.IsNullOrEmpty(interfaceErrCode))
            {
                LastErrorCode = interfaceErrCode;
                LastErrorMessage = interfaceErrMsg;
                LastDiagnostic = "[" + interfaceErrCode + "]" + interfaceErrMsg;
                return "";
            }

            // 接口正常，只是码值没匹配：算"查询成功但无此码"，列出当前账套真实编码，用户一眼知道该输什么
            LastSucceeded = true;
            LastErrorCode = "NOT_FOUND";
            LastDiagnostic = "[" + trimmed + "]在金蝶物料里没找到；当前账套可用编码："
                + DescribeRows(firstJson, 4);
            return "";
        }

        private string QueryMaterialPage(int page)
        {
            var ps = new List<LrParam>();
            ps.Add(Param("page", page.ToString()));
            ps.Add(Param("page_size", "100"));
            ps.Add(Param("show_units", "true"));
            return new LrGetMtrlDatum().GetDatumX(_clientId, _clientSecret, _appToken, ps, "jdy/v2/bd/material");
        }

        private static List<object> MatchRows(object[] rows, string code, bool fuzzy)
        {
            var list = new List<object>();
            foreach (object o in rows)
            {
                var row = o as Dictionary<string, object>;
                if (row == null) continue;
                string num = ToStr(GetVal(row, "number"));
                string bc = ToStr(GetVal(row, "barcode"));
                string nm = ToStr(GetVal(row, "name"));
                bool hit = fuzzy
                    ? (num.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0
                       || nm.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0)
                    : (string.Equals(num, code, StringComparison.OrdinalIgnoreCase)
                       || string.Equals(bc, code, StringComparison.OrdinalIgnoreCase));
                if (hit) list.Add(row);
            }
            return list;
        }

        /// <summary>
        /// 容错匹配：去掉易误输入的字母 V 后再按 number/barcode 精确匹配。
        /// 场景：账套真实编码是 M00001，用户输成 MV00001（多打一个 V）导致精确/包含都落空。
        /// 只在调用方确认"唯一命中"时才采用，避免串料。stripped 为空或与原文相同则不产生结果。
        /// </summary>
        private static List<object> MatchRowsIgnoringV(object[] rows, string code)
        {
            var list = new List<object>();
            string stripped = code.Replace("V", "").Replace("v", "");
            if (string.IsNullOrEmpty(stripped) || string.Equals(stripped, code, StringComparison.Ordinal))
                return list;
            foreach (object o in rows)
            {
                var row = o as Dictionary<string, object>;
                if (row == null) continue;
                string num = ToStr(GetVal(row, "number"));
                string bc = ToStr(GetVal(row, "barcode"));
                if (string.Equals(num, stripped, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(bc, stripped, StringComparison.OrdinalIgnoreCase))
                    list.Add(row);
            }
            return list;
        }

        private static string WrapRows(List<object> matched)
        {
            var data = new Dictionary<string, object>();
            data["total"] = matched.Count;
            data["rows"] = matched.ToArray();
            var root = new Dictionary<string, object>();
            root["data"] = data;
            return NewSerializer().Serialize(root);
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
    }
}
