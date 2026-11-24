using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 拍平后的单个字段
    /// </summary>
    public class FlatField
    {
        public string Name;
        public string Value;
        public string Type;
    }

    /// <summary>
    /// JSON动态解析工具：负责把任意结构的接口返回JSON拆成“数据行 + 拍平字段 + 内容哈希”，
    /// 供 EF Core 动态入库使用。
    /// </summary>
    public static class JsonFlattener
    {
        // 可能作为业务主键的字段名（按优先级）
        private static readonly string[] KeyCandidates = new string[]
        {
            "id", "material_id", "bill_no", "billid", "bill_id", "number", "code",
            "customer_id", "supplier_id", "store_id", "dept_id", "emp_id", "unit_id",
            "group_id", "material_group_id", "aux_id", "order_id", "entry_id",
            "full_number", "name"
        };

        // 可能作为显示名称的字段名（按优先级）
        private static readonly string[] TitleCandidates = new string[]
        {
            "name", "full_name", "material_name", "bill_name", "customer_name",
            "supplier_name", "store_name", "dept_name", "emp_name", "number", "code"
        };

        /// <summary>
        /// 从接口返回的JSON中提取数据行。
        /// 支持：data.rows / data.list / data 数组 / 根数组 / 单对象 / 非JSON文本。
        /// </summary>
        public static List<JObject> ExtractRows(string json)
        {
            List<JObject> rows = new List<JObject>();
            if (string.IsNullOrWhiteSpace(json)) return rows;

            JToken root;
            try
            {
                root = JToken.Parse(json);
            }
            catch
            {
                // 非JSON（例如异常文本）也照样入库，保证“输出的信息”都能落库
                JObject synthetic = new JObject();
                synthetic["_raw_response"] = json.Length > 8000 ? json.Substring(0, 8000) : json;
                rows.Add(synthetic);
                return rows;
            }

            if (root.Type == JTokenType.Array)
            {
                AddObjects(rows, root);
            }
            else if (root.Type == JTokenType.Object)
            {
                JObject obj = (JObject)root;
                JToken data = PickToken(obj, "data", "result", "Data", "Result");
                JToken container = data ?? obj;

                JToken rowsToken = null;
                if (container.Type == JTokenType.Object)
                {
                    rowsToken = PickToken((JObject)container, "rows", "list", "items", "records", "data");
                }
                else if (container.Type == JTokenType.Array)
                {
                    rowsToken = container;
                }

                if (rowsToken != null && rowsToken.Type == JTokenType.Array)
                {
                    AddObjects(rows, rowsToken);
                }
                else if (container.Type == JTokenType.Object)
                {
                    rows.Add((JObject)container);
                }
                else if (container.Type == JTokenType.Array)
                {
                    AddObjects(rows, container);
                }
                else
                {
                    // 例如 data 是标量：连外层一起存
                    rows.Add(obj);
                }
            }
            else
            {
                JObject synthetic = new JObject();
                synthetic["_raw_response"] = root.ToString();
                rows.Add(synthetic);
            }

            if (rows.Count == 0)
            {
                // 接口返回空数组：把外层对象存一条，便于记录“该接口返回空数据”这一事实
                if (root.Type == JTokenType.Object) rows.Add((JObject)root);
            }
            return rows;
        }

        private static void AddObjects(List<JObject> rows, JToken array)
        {
            foreach (JToken item in array.Children())
            {
                if (item.Type == JTokenType.Object) rows.Add((JObject)item);
                else
                {
                    JObject wrap = new JObject();
                    wrap["_value"] = item.ToString();
                    rows.Add(wrap);
                }
            }
        }

        private static JToken PickToken(JObject obj, params string[] names)
        {
            foreach (string name in names)
            {
                JToken t = obj.GetValue(name, StringComparison.OrdinalIgnoreCase);
                if (t != null && t.Type != JTokenType.Null) return t;
            }
            return null;
        }

        /// <summary>
        /// 把一行数据拍平成字段列表：顶层字段逐个存行；嵌套对象/数组整体存一行 JSON 文本，
        /// 不再递归展开成 a.b / a[0].b，避免字段明细表被拆成大量碎行、字段名难读。
        /// </summary>
        public static List<FlatField> Flatten(JObject row)
        {
            List<FlatField> list = new List<FlatField>();
            foreach (JProperty p in row.Properties())
            {
                Walk(list, p.Name, p.Value);
            }
            return list;
        }

        private static void Walk(List<FlatField> list, string path, JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                list.Add(new FlatField { Name = Truncate(path, 300), Value = null, Type = "null" });
                return;
            }
            if (token is JValue)
            {
                list.Add(new FlatField
                {
                    Name = Truncate(path, 300),
                    Value = Truncate(token.ToString(), 4000),
                    Type = TypeOf(token)
                });
                return;
            }
            if (token.Type == JTokenType.Object)
            {
                // 嵌套对象整体存一行 JSON 文本，不再递归展开子字段
                // （原来会产生 primary_unit.unit_id 等碎行，导致字段明细表杂乱）
                list.Add(new FlatField { Name = Truncate(path, 300), Value = Truncate(token.ToString(Newtonsoft.Json.Formatting.None), 4000), Type = "object" });
                return;
            }
            if (token.Type == JTokenType.Array)
            {
                // 数组整体存一行 JSON 文本，不再逐元素展开
                // （原来会产生 alt_units[0].unit_id 等碎行）
                list.Add(new FlatField { Name = Truncate(path, 300), Value = Truncate(token.ToString(Newtonsoft.Json.Formatting.None), 4000), Type = "array" });
                return;
            }
            list.Add(new FlatField { Name = Truncate(path, 300), Value = Truncate(token.ToString(), 4000), Type = TypeOf(token) });
        }

        private static string TypeOf(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Integer: return "number";
                case JTokenType.Float: return "number";
                case JTokenType.Boolean: return "bool";
                case JTokenType.Date: return "date";
                case JTokenType.String: return "string";
                default: return token.Type.ToString().ToLowerInvariant();
            }
        }

        /// <summary>
        /// 自动识别业务主键：优先 id/number/bill_no 等字段，识别不到则用内容哈希（同样能去重）
        /// </summary>
        public static string PickBizKey(JObject row, string hash)
        {
            foreach (string key in KeyCandidates)
            {
                JToken t = row.GetValue(key, StringComparison.OrdinalIgnoreCase);
                if (t != null && t.Type != JTokenType.Null && t.Type != JTokenType.Object && t.Type != JTokenType.Array)
                {
                    string v = t.ToString().Trim();
                    if (!string.IsNullOrEmpty(v)) return Truncate(key + "=" + v, 200);
                }
            }
            return Truncate("hash=" + hash, 200);
        }

        public static string PickTitle(JObject row)
        {
            foreach (string key in TitleCandidates)
            {
                JToken t = row.GetValue(key, StringComparison.OrdinalIgnoreCase);
                if (t != null && t.Type == JTokenType.String)
                {
                    string v = t.ToString().Trim();
                    if (!string.IsNullOrEmpty(v)) return Truncate(v, 300);
                }
            }
            return null;
        }

        /// <summary>
        /// 计算内容哈希：先对JSON做“键排序规范化”，再SHA256，
        /// 这样字段顺序不同、缩进不同的相同数据会得到同一个哈希 => 相同数据仅存一次。
        /// </summary>
        public static string ComputeHash(JToken token)
        {
            string canonical = Normalize(token).ToString(Newtonsoft.Json.Formatting.None);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                StringBuilder sb = new StringBuilder(64);
                for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        private static JToken Normalize(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject result = new JObject();
                foreach (JProperty p in ((JObject)token).Properties().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    result[p.Name] = Normalize(p.Value);
                }
                return result;
            }
            if (token.Type == JTokenType.Array)
            {
                JArray arr = new JArray();
                foreach (JToken item in token.Children()) arr.Add(Normalize(item));
                return arr;
            }
            return token.DeepClone();
        }

        private static string Truncate(string value, int maxLength)
        {
            if (value == null) return null;
            if (value.Length <= maxLength) return value;
            return value.Substring(0, maxLength);
        }
    }
}
