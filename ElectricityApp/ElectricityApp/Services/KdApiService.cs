using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Script.Serialization;

namespace ElectricityApp
{
    /// <summary>
    /// 金蝶 API 查询服务（Smobiler 手机端复用）。
    /// 凭证从 app.config 的 appSettings 读取；app-token 进程内缓存，满 20 小时自动续期。
    /// 底层取数完全复用 LrGetToken 的 GetAppSecretB / GetToken / GetDatumX，不重造签名逻辑。
    /// 注意：本文件刻意不用 Newtonsoft.Json.Linq 类型——Smobiler.Plugins.dll 内嵌了一份 internal 的
    /// Newtonsoft.Json，会与包引用冲突导致 CS0122（JToken 不可访问）。改用 .NET 自带的 JavaScriptSerializer。
    ///
    /// 本类按职责拆为 5 个 partial 文件（同 namespace、同类名，对外完全等价）：
    ///   KdApiService.cs          门面：凭证、通用工具、状态属性
    ///   KdApiService.Token.cs    token 缓存与续期
    ///   KdApiService.Query.cs    物料查询、翻页、编码匹配
    ///   KdApiService.Parse.cs    JSON 解析、响应判定、取数据行
    ///   KdApiService.Diagnose.cs 诊断信息、DataTable 构建、字段兜底
    /// </summary>
    public partial class KdApiService
    {
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

        /// <summary>最近一次查询的诊断信息（token 状态 / 接口返回片段），查不到时显示在界面上。</summary>
        public string LastDiagnostic { get; private set; }

        /// <summary>最近一次查询的"接口层"成功标志：true=接口通且无业务错误码（可能无匹配数据）；false=token/网络/错误码失败。</summary>
        public bool LastSucceeded { get; private set; }

        /// <summary>最近一次查询的业务错误码（成功时为空）。</summary>
        public string LastErrorCode { get; private set; }

        /// <summary>最近一次查询的业务错误信息（成功时为空）。</summary>
        public string LastErrorMessage { get; private set; }

        /// <summary>第 4 步：扫码上报。待确认单据类型与 JSON 样例后实现。</summary>
        public string SubmitBill(string billJson)
        {
            throw new NotImplementedException();
        }
    }
}
