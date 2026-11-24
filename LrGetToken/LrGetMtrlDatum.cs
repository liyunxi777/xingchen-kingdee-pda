using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LrGetToken
{
	public class LrGetMtrlDatum
	{
		/// <summary>
		/// 请求观测回调：由宿主程序（如 FrmTest）挂接，用于记录每次真实 API 请求。
		/// 签名：(接口路径, 请求参数文本, 返回JSON, 是否成功, 失败原因)。
		/// 类库本身不引用宿主的数据层，避免循环依赖；回调为 null 时不影响任何取数。
		/// </summary>
		public static Action<string, string, string, bool, string> RequestObserver = null;

		public string GetDatum(string clientId, string clientSecret, string appKey, string appSecret, string strToken, string mtrlCode,
			string mtrlName, string strPath, string pageCnt, string pageSize)
		{
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Get;
				apiRequest.Params.Add("enable", "1");
				apiRequest.Params.Add("page", pageCnt);
				apiRequest.Params.Add("page_size", pageSize);
				//apiRequest.Params.Add("search", "包引");
				apiRequest.Path = strPath;
				apiRequest.Header.Add("app-token", strToken);
				apiRequest.Header.Add("X-GW-Router-Addr", "https://tf.jdy.com");
				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.HttpGet(apiRequest, api_nonce.ToString(), timeStamp.ToString());
				return strJson;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}
		}

		public string GetDatumXX(string clientId, string clientSecret, string strToken, List<LrParam> paramLst, string strPath)
		{
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Get;
				apiRequest.Path = strPath;
				foreach (LrParam lst in paramLst)
				{
					apiRequest.Params.Add(lst.pKey, lst.pValue);
				}
				apiRequest.Header.Add("app-token", strToken);
				apiRequest.Header.Add("X-GW-Router-Addr", "https://tf.jdy.com");
				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.HttpGetX(apiRequest, api_nonce.ToString(), timeStamp.ToString());
				return strJson;
			}
			catch (Exception ex)
			{
				return "";
			}
		}

		/// <summary>
		/// 常用取数通道：所有业务按钮与 Tab 自动刷新都走这里。
		/// 在 finally 中通过 RequestObserver 上报一次请求的参数、结果、成败，
		/// 宿主程序借此写入请求日志表；回调异常被吞掉，绝不影响取数。
		/// </summary>
		public string GetDatumX(string clientId, string clientSecret, string strToken, List<LrParam> paramLst, string strPath)
		{
			string paramText = BuildParamText(paramLst);
			bool ok = false;
			string errMsg = "";
			string strJson = "";
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Get;
				foreach (LrParam lst in paramLst)
				{
					apiRequest.Params.Add(lst.pKey, lst.pValue);
				}
				apiRequest.Path = strPath;
				apiRequest.Header.Add("app-token", strToken);
				apiRequest.Header.Add("X-GW-Router-Addr", "https://tf.jdy.com");
				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				strJson = client.HttpGetX(apiRequest, api_nonce.ToString(), timeStamp.ToString());
				ok = !string.IsNullOrWhiteSpace(strJson);
				if (!ok) errMsg = "接口返回空内容（可能 token 过期或网络失败）";
				return strJson;
			}
			catch (Exception ex)
			{
				errMsg = ex.Message;
				return "";
			}
			finally
			{
				FireObserver(strPath, paramText, strJson, ok, errMsg);
			}
		}

		/// <summary>把参数列表拼成可读的 key=value&... 文本。</summary>
		private static string BuildParamText(List<LrParam> paramLst)
		{
			if (paramLst == null) return "";
			var sb = new StringBuilder();
			foreach (LrParam p in paramLst)
			{
				if (p == null) continue;
				if (sb.Length > 0) sb.Append("&");
				sb.Append(p.pKey).Append("=").Append(p.pValue);
			}
			return sb.ToString();
		}

		/// <summary>安全触发观测回调：为 null 或抛异常都不影响取数主流程。</summary>
		private static void FireObserver(string path, string paramText, string json, bool ok, string errMsg)
		{
			try
			{
				Action<string, string, string, bool, string> observer = RequestObserver;
				if (observer != null) observer(path, paramText, json, ok, errMsg);
			}
			catch { }
		}

		public string GetMtrlDatum(string clientId, string clientSecret, string strToken, int startTime, int endTime, string strPath)
		{
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Get;
				apiRequest.Path = strPath;
				apiRequest.Header.Add("app-token", strToken);
				apiRequest.Header.Add("X-GW-Router-Addr", "https://tf.jdy.com");
				apiRequest.Params.Add("enable", "1");
				apiRequest.Params.Add("page", "1");
				apiRequest.Params.Add("page_size", "100");
				//apiRequest.Params.Add("appFlag", "1");

				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.HttpGet(apiRequest, api_nonce.ToString(), timeStamp.ToString());
				return strJson;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}
		}

		public string GetToken(string clientId, string clientSecret, string app_key, string appsecret)
		{
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Get;
				apiRequest.Params.Add("app_key", app_key);
				apiRequest.Params.Add("app_signature", SHAHelper.HmacSHA256(appsecret, app_key));
				apiRequest.Path = "jdyconnector/app_management/kingdee_auth_token";
				
				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.HttpGet(apiRequest, api_nonce.ToString(), timeStamp.ToString());
				JObject jo = (JObject)JsonConvert.DeserializeObject(strJson);
				strJson = jo["data"]["app-token"].ToString();
				return strJson;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}
		}
	}
}