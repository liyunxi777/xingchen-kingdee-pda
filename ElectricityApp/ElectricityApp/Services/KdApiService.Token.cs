using System;
using System.Collections.Generic;
using LrGetToken;

namespace ElectricityApp
{
    /// <summary>token 部分：进程内缓存、20 小时自动续期、底层复用 LrGetToken 签名逻辑。</summary>
    public partial class KdApiService
    {
        private static readonly object _tokenLock = new object();
        private static string _appToken = "";
        private static DateTime _tokenIssuedAt = DateTime.MinValue;
        private static readonly TimeSpan TOKEN_REFRESH_BEFORE = TimeSpan.FromHours(20);

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
    }
}
