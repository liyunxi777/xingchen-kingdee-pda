using System;
using System.Collections.Generic;

namespace ElectricityApp
{
    /// <summary>解析部分：接口响应成功判定、数据行提取。兼容 K3Cloud/BOS 与精斗云 jdy 两种返回结构。</summary>
    public partial class KdApiService
    {
        /// <summary>
        /// 判断接口返回是否"业务成功"——依据是 IsSuccess / ErrorCode / 业务错误码 / 有效 rows，
        /// 不是"字符串是不是 JSON / 是不是空串"。兼容两种金蝶返回结构：
        ///   1) K3Cloud/BOS：Result.ResponseStatus.IsSuccess(bool) / ErrorCode，错误文本在 Errors[]；
        ///   2) 精斗云 jdy（本项目实际使用）：根级 errorcode/errcode + message，正常响应带 rows（可为空数组）。
        /// 注意：rows 为空数组代表"查询成功但无数据"，仍算接口有效，与"接口失败"严格区分。
        /// </summary>
        private static bool CheckResponse(string json, out string errorCode, out string errorMessage)
        {
            errorCode = "";
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(json))
            {
                errorCode = "EMPTY";
                errorMessage = "接口无返回（token 过期或网络失败）";
                return false;
            }

            Dictionary<string, object> root = ParseRoot(json);
            if (root == null)
            {
                errorCode = "INVALID_JSON";
                errorMessage = "返回内容不是合法 JSON";
                return false;
            }

            // 结构一：Result.ResponseStatus（K3Cloud / BOS WebAPI）
            Dictionary<string, object> status = null;
            object resultObj = GetVal(root, "Result");
            if (resultObj == null) resultObj = GetVal(root, "result");
            Dictionary<string, object> result = resultObj as Dictionary<string, object>;
            if (result != null)
            {
                object stObj = GetVal(result, "ResponseStatus");
                if (stObj == null) stObj = GetVal(result, "response_status");
                status = stObj as Dictionary<string, object>;
            }
            if (status != null)
            {
                object succObj = GetVal(status, "IsSuccess");
                if (succObj == null) succObj = GetVal(status, "isSuccess");
                object codeObj = GetVal(status, "ErrorCode");
                if (codeObj == null) codeObj = GetVal(status, "errorCode");

                bool succ = succObj is bool && (bool)succObj;
                if (!succ && succObj != null)
                {
                    bool parsed;
                    if (bool.TryParse(ToStr(succObj), out parsed)) succ = parsed;
                }
                string ec = ToStr(codeObj);
                // IsSuccess=true 或 错误码为空/0，均视为成功
                if (succ || string.IsNullOrEmpty(ec) || ec == "0")
                {
                    errorCode = "";
                    return true;
                }
                errorCode = ec;
                errorMessage = FirstError(status);
                return false;
            }

            // 结构二：精斗云 jdy —— 根级业务错误码
            string[] codeKeys = { "errorcode", "error_code", "errcode" };
            string[] msgKeys = { "message", "errmsg", "error_message", "msg" };
            string bizCode = "";
            foreach (string k in codeKeys)
            {
                object v = GetVal(root, k);
                if (v == null) continue;
                bizCode = ToStr(v);
                break;
            }
            if (!string.IsNullOrEmpty(bizCode) && bizCode != "0"
                && !bizCode.Equals("success", StringComparison.OrdinalIgnoreCase))
            {
                errorCode = bizCode;
                foreach (string k in msgKeys)
                {
                    object v = GetVal(root, k);
                    if (v != null) { errorMessage = ToStr(v); break; }
                }
                return false;
            }

            // 兜底：能否解析出 rows —— 空 rows 也是"查询成功但无数据"，接口仍有效
            int total;
            object[] rows = GetRows(json, out total);
            if (rows == null)
            {
                errorCode = "NO_ROWS";
                errorMessage = "返回中无有效数据行";
                return false;
            }
            return true;
        }

        /// <summary>取 ResponseStatus.Errors 第一条错误文本。</summary>
        private static string FirstError(Dictionary<string, object> status)
        {
            object errObj = GetVal(status, "Errors");
            if (errObj == null) errObj = GetVal(status, "errors");
            object[] errs = errObj as object[];
            if (errs == null || errs.Length == 0) return "";
            Dictionary<string, object> first = errs[0] as Dictionary<string, object>;
            if (first == null) return ToStr(errs[0]);
            object msg = GetVal(first, "Message");
            if (msg == null) msg = GetVal(first, "message");
            return ToStr(msg);
        }

        /// <summary>
        /// 取数据行：兼容金蝶所有返回结构。
        /// 关键修正：物料列表接口返回 {data:{rows:[...],count:...}}，
        /// 但 search 等接口返回 {rows:[...],count:...}（rows 直接在根节点，无 data 包裹）。
        /// 之前只认 data.rows，导致 search 返回的内容被解析成 0 行 → 格子全空。
        /// </summary>
        private static object[] GetRows(string json, out int total)
        {
            total = 0;
            Dictionary<string, object> root = ParseRoot(json);
            if (root == null) return null;

            // 定位装 rows 的容器：优先 data 节点；没有 data 就用根节点
            Dictionary<string, object> container = null;
            object dataObj = GetVal(root, "data");
            if (dataObj is object[])
            {
                // 结构一：data 直接是数组
                object[] direct = (object[])dataObj;
                total = direct.Length;
                return direct;
            }
            if (dataObj is Dictionary<string, object>)
                container = (Dictionary<string, object>)dataObj;
            else
                container = root;   // 无 data 节点：rows 直接在根节点（search 接口）

            // total 可能在 total / count 字段
            object totalObj = GetVal(container, "total");
            if (totalObj == null) totalObj = GetVal(container, "count");
            if (totalObj != null)
            {
                int t;
                if (int.TryParse(ToStr(totalObj), out t)) total = t;
            }

            object[] rows = GetVal(container, "rows") as object[];
            if (rows != null) return rows;

            // data 本身是单条对象（无 rows），包成一行；根节点无 rows 则说明没有数据
            if (dataObj is Dictionary<string, object>) return new object[] { container };
            return null;
        }

        private static bool HasRows(string json)
        {
            int total;
            object[] data = GetRows(json, out total);
            return data != null && data.Length > 0;
        }
    }
}
