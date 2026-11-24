using System;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 请求日志写入器：把一次真实 API 请求落库到 api_request_log。
    /// 设备编号优先取 App.config 的 appSettings/DeviceId，未配置时用本机机器名兜底。
    /// 全部异常静默吞掉——日志失败绝不影响取数主流程。
    /// </summary>
    public static class RequestLogStore
    {
        private static readonly object _writeLock = new object();
        private static string _deviceId = null;

        /// <summary>当前设备编号（PDA 编号），进程内缓存。</summary>
        public static string DeviceId
        {
            get
            {
                if (_deviceId != null) return _deviceId;
                try
                {
                    string cfg = ConfigurationManager.AppSettings["DeviceId"];
                    _deviceId = string.IsNullOrWhiteSpace(cfg)
                        ? Environment.MachineName
                        : cfg.Trim();
                }
                catch
                {
                    _deviceId = Environment.MachineName;
                }
                return _deviceId;
            }
        }

        /// <summary>
        /// 记录一次请求。用独立 DbContext，加锁串行化避免并发写冲突。
        /// </summary>
        /// <param name="endpoint">接口路径</param>
        /// <param name="endpointName">按钮/接口中文名</param>
        /// <param name="paramText">请求参数文本（key=value&...）</param>
        /// <param name="responseJson">金蝶返回原始 JSON</param>
        /// <param name="success">是否成功</param>
        /// <param name="message">失败原因/备注</param>
        public static void Write(string endpoint, string endpointName, string paramText,
            string responseJson, bool success, string message)
        {
            try
            {
                int rows = ParseResponseRows(responseJson);
                lock (_writeLock)
                {
                    using (var db = new XcApiDbContext())
                    {
                        db.ApiRequestLogs.Add(new ApiRequestLog
                        {
                            DeviceId = DeviceId,
                            Endpoint = endpoint ?? "",
                            EndpointName = endpointName ?? "",
                            RequestParams = paramText ?? "",
                            RequestTime = DateTime.Now,
                            ResponseRows = rows,
                            Status = success ? "成功" : "失败",
                            Message = message ?? ""
                        });
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                // 日志写入失败不影响业务
            }
        }

        /// <summary>
        /// 从金蝶返回 JSON 解析数据条数：兼容 data 数组、data.total、data.count、data.rows 等结构。
        /// 解析不到返回 -1。
        /// </summary>
        public static int ParseResponseRows(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return -1;
            try
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(json);
                if (jo == null) return -1;
                JToken data = jo["data"];
                if (data == null || data.Type == JTokenType.Null) return -1;
                if (data is JArray) return ((JArray)data).Count;
                if (data is JObject)
                {
                    JObject d = (JObject)data;
                    JToken cnt = d["total"] ?? d["count"] ?? d["Total"] ?? d["Count"];
                    if (cnt != null)
                    {
                        int n;
                        if (int.TryParse(cnt.ToString(), out n)) return n;
                    }
                    JToken rows = d["rows"] ?? d["list"] ?? d["items"] ?? d["data"];
                    if (rows is JArray) return ((JArray)rows).Count;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }
    }
}
