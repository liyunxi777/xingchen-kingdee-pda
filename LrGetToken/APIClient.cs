using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
namespace LrGetToken
{
	public class APIClient
	{
		/// <summary>
		/// 请求服务器域名,默认https://api.kingdee.com/
		/// </summary>
		private string _baseUrl = "https://api.kingdee.com/";

		/// <summary>
		/// 客户Id
		/// </summary>
		private string _clientId = "";

		/// <summary>
		/// 客户秘钥
		/// </summary>
		private string _clientSecret = "";

		/// <summary>
		/// 超时时间（秒）
		/// </summary>
		public int Timeout { get { return 10; } set { value = 10; } }

		/// <summary>
		/// 构造一个API客户端对象
		/// </summary>
		/// <param name="clientId">客户Id</param>
		/// <param name="clientSecret">客户秘钥</param>
		/// /// <param name="baseUrl">请求域名</param>
		public APIClient(string clientId, string clientSecret, string baseUrl)
		{
			this._clientId = clientId;
			this._clientSecret = clientSecret;
			this._baseUrl = baseUrl;
		}

		/// <summary>
		/// 构造一个API客户端对象
		/// </summary>
		/// <param name="clientId">客户Id</param>
		/// <param name="clientSecret">客户秘钥</param>
		public APIClient(string clientId, string clientSecret)
		{
			this._clientId = clientId;
			this._clientSecret = clientSecret;
		}

		/// <summary>
		/// 发起请求
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> Execute(APIRequest request, string api_nonce, string timeStamp)
		{
			using (HttpClient _client = new HttpClient())
			{
				_client.Timeout = TimeSpan.FromSeconds((double)Timeout);
				BuildRequestHeader(_client.DefaultRequestHeaders, request, api_nonce, timeStamp);

				var uri = _baseUrl.TrimEnd('/') + "/" + request.Path + "?" + GetQueryString(request);
				if (request.Method == HttpMethod.Post)
				{
					if (string.IsNullOrEmpty(request.File))
					{
						var json = request.Body == null ? string.Empty : JsonConvert.SerializeObject(request.Body);
						return await _client.PostAsync(uri, new StringContent(json, Encoding.UTF8, request.ContentType));
					}
					else
					{
						//文件上传
						string boundary = "----WebKitFormBoundary{DateTime.Now.Ticks:x}";
						MultipartFormDataContent content = new MultipartFormDataContent(boundary);
						if (!File.Exists(request.File))
						{
							throw new Exception("路径不存在:{request.File}");
						}

						using (FileStream fStream = File.Open(request.File, FileMode.Open, FileAccess.Read))
						{
							content.Add(new StreamContent(fStream, (int)fStream.Length), "file", Path.GetFileName(request.File));
							return await _client.PostAsync(uri, content);
						}
					}
				}
				else if (request.Method == HttpMethod.Get)
				{
					return await _client.GetAsync(uri);
				}
				else if (request.Method == HttpMethod.Put)
				{
					var json = request.Body == null ? string.Empty : JsonConvert.SerializeObject(request.Body);
					return await _client.PutAsync(uri, new StringContent(json, Encoding.UTF8, request.ContentType));
				}
				else if (request.Method == HttpMethod.Delete)
				{
					return await _client.DeleteAsync(uri);
				}
				else
				{
					throw new Exception("Method:{request.Method} 参数不支持!");
				}
			}
		}

		public string HttpPost(APIRequest request, string ClientID, string outerInstanceId, string api_nonce, string timeStamp)
		{
			try
			{
				var uri = _baseUrl.TrimEnd('/') + "/" + request.Path + "?outerInstanceId=" + outerInstanceId;
				HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(uri);
				request1.ContentType = "application/json";
				request1.Method = "POST";
				//request1.Headers.Add("Content-Type", "application/json");
				request1.Headers.Add("X-Api-ClientID", ClientID);
				request1.Headers.Add("X-Api-Auth-Version", "2.0");
				request1.Headers.Add("X-Api-TimeStamp", timeStamp.ToString());
				request1.Headers.Add("X-Api-SignHeaders", "X-Api-TimeStamp,X-Api-Nonce");
				request1.Headers.Add("X-Api-Nonce", api_nonce.ToString());
				request1.Headers.Add("X-Api-Signature", GetSignature(request, api_nonce.ToString(), timeStamp.ToString()));
				//string postData = "key1=value1&key2=value2";

				//GetHeader(request1, request);
				//Stream requestStream = request1.GetRequestStream();
				//StreamWriter streamWriter = new StreamWriter(requestStream);
				//streamWriter.Write(postDataStr);
				//streamWriter.Close();

				HttpWebResponse response = (HttpWebResponse)request1.GetResponse();

				Stream responseStream = response.GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream);
				string retString = streamReader.ReadToEnd();
				streamReader.Close();
				responseStream.Close();

				return retString;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}
		}
		//post
		public string HttpPost(string strPath, string ClientID, string appToken, string api_nonce, string timeStamp, object body, string contentType = "application/json", int timeOut = 500)
		{
			try
			{
				var url = _baseUrl.TrimEnd('/') + "/" + strPath;
				string quertString = string.Empty;

				using (HttpClient client = new HttpClient())
				{
					client.Timeout = new TimeSpan(0, 0, timeOut);
					client.DefaultRequestHeaders.Add("X-Api-ClientID", _clientId);
					client.DefaultRequestHeaders.Add("X-Api-Auth-Version", "2.0");
					client.DefaultRequestHeaders.Add("X-Api-TimeStamp", timeStamp.ToString());
					client.DefaultRequestHeaders.Add("X-Api-SignHeaders", "X-Api-TimeStamp,X-Api-Nonce");
					client.DefaultRequestHeaders.Add("X-Api-Nonce", api_nonce.ToString());
					var signature = GetSignature("post", strPath, quertString, api_nonce.ToString(), timeStamp.ToString());
					client.DefaultRequestHeaders.Add("X-Api-Signature", signature);
					client.DefaultRequestHeaders.Add("app-token", appToken);
					client.DefaultRequestHeaders.Add("X-GW-Router-Addr", "https://tf.jdy.com");

					using (HttpContent httpContent = new StringContent(body.ToString(), Encoding.UTF8))
					{
						if (contentType != null)
							httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
						HttpResponseMessage response = client.PostAsync(url, httpContent).Result;
						var res = response.Content.ReadAsStringAsync().Result;
						return res;
					}
				}
			}
			catch (WebException ex)
			{
				// 处理 Web 异常（重点：提取服务器返回的 519 响应详情）
				Console.WriteLine("请求错误: " + ex.Message);
				return "";
			}
		}

		public string SaveBill(APIRequest request, string ClientID, string appToken, string api_nonce, string timeStamp,string billContent)
		{
			try
			{
				var uri = _baseUrl.TrimEnd('/') + "/" + request.Path;
				HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(uri);
				request1.ContentType = "application/json";
				request1.Method = "POST";
				//request1.Headers.Add("Content-Type", "application/json");
				request1.Headers.Add("X-Api-ClientID", ClientID);
				request1.Headers.Add("X-Api-Auth-Version", "2.0");
				request1.Headers.Add("X-Api-SignHeaders", "X-Api-TimeStamp,X-Api-Nonce");
				request1.Headers.Add("X-Api-TimeStamp", timeStamp.ToString());
				request1.Headers.Add("X-Api-Nonce", api_nonce.ToString());
				request1.Headers.Add("X-Api-Signature", GetSignature(request, api_nonce.ToString(), timeStamp.ToString()));
				request1.Headers.Add("app-token", appToken);
				request1.Headers.Add("X-GW-Router-Addr", "https://tf.jdy.com");
				//string postData = "key1=value1&key2=value2";
				if (billContent != "")
				{
					byte[] byteArray = Encoding.UTF8.GetBytes(billContent);
					request1.ContentLength = byteArray.Length;
					Stream dataStream = request1.GetRequestStream();
					// Write the data to the request stream
					dataStream.Write(byteArray, 0, byteArray.Length);

					// Close the Stream object
					dataStream.Close();

					//Stream requestStream = request1.GetRequestStream();
					//StreamWriter streamWriter = new StreamWriter(requestStream);
					//streamWriter.Write(billContent);
					//streamWriter.Close();
				}
				//GetHeader(request1, request);
				//Stream requestStream = request1.GetRequestStream();
				//StreamWriter streamWriter = new StreamWriter(requestStream);
				//streamWriter.Write(postDataStr);
				//streamWriter.Close();
				HttpWebResponse response = (HttpWebResponse)request1.GetResponse();
				Stream responseStream = response.GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream);
				string retString = streamReader.ReadToEnd();
				streamReader.Close();
				responseStream.Close();

				return retString;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return ex.Message;
			}
		}
		
		public string HttpGet(APIRequest request, string api_nonce, string timeStamp)
		{
			try
			{
				var uri = _baseUrl.TrimEnd('/') + "/" + request.Path + "?" + GetQueryString(request);
				HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(uri);
				GetHeader(request1, request, api_nonce, timeStamp);
				HttpWebResponse response = (HttpWebResponse)request1.GetResponse();
				Stream myResponseStream = response.GetResponseStream();
				StreamReader streamReader = new StreamReader(myResponseStream);
				string retString = streamReader.ReadToEnd();
				streamReader.Close();
				myResponseStream.Close();
				return retString;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}
		}

		public string HttpGetX(APIRequest request, string api_nonce, string timeStamp)
		{
			try
			{
				var uri = _baseUrl.TrimEnd('/') + "/" + request.Path + "?" + GetQueryString(request);
				HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(uri);
				GetHeader(request1, request, api_nonce, timeStamp);
				HttpWebResponse response = (HttpWebResponse)request1.GetResponse();
				Stream myResponseStream = response.GetResponseStream();
				StreamReader streamReader = new StreamReader(myResponseStream);
				string retString = streamReader.ReadToEnd();
				streamReader.Close();
				myResponseStream.Close();
				return retString;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}
		}

		private void GetHeader(HttpWebRequest request1, APIRequest request, string api_nonce, string timeStamp)
		{
			//var api_nonce = new Random().Next(100000000, 999999999);
			//var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;

			request1.Headers.Add("X-Api-ClientID", _clientId);
			request1.Headers.Add("X-Api-Auth-Version", "2.0");
			request1.Headers.Add("X-Api-TimeStamp", timeStamp.ToString());
			request1.Headers.Add("X-Api-SignHeaders", "X-Api-Nonce,X-Api-TimeStamp");
			request1.Headers.Add("X-Api-Nonce", api_nonce.ToString());
			request1.Headers.Add("X-Api-Signature", GetSignature(request, api_nonce.ToString(), timeStamp.ToString()));
			//headers.Add("X-GW-Router-Addr", "https://tf.jdy.com/");
			if (request.Header.Any())
			{
				foreach (var hk in request.Header.Keys)
				{
					request1.Headers.Add(hk, request.Header[hk]);
				}
			}
		}

		private void BuildRequestHeader(HttpRequestHeaders headers, APIRequest request, string api_nonce, string timeStamp)
		{
			//var api_nonce = new Random().Next(100000000, 999999999);
			//var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;

			headers.Add("X-Api-Nonce", api_nonce.ToString());
			headers.Add("X-Api-Auth-Version", "2.0");
			headers.Add("X-Api-TimeStamp", timeStamp.ToString());
			headers.Add("X-Api-ClientID", _clientId);
			headers.Add("X-Api-Signature", GetSignature(request, api_nonce.ToString(), timeStamp.ToString()));
			headers.Add("X-Api-SignHeaders", "X-Api-Nonce,X-Api-TimeStamp");
			//headers.Add("X-GW-Router-Addr", "https://tf.jdy.com/");

			if (request.Header.Any())
			{
				foreach (var hk in request.Header.Keys)
				{
					headers.Add(hk, request.Header[hk]);
				}
			}
		}

		private string GetSignature(APIRequest request, string nonce, string timestamp)
		{
			var querySign = GetQueryString(request, true);

			StringBuilder sb = new StringBuilder();
			sb.Append(request.Method.ToString().ToUpper());
			sb.Append('\n');
			sb.Append(PathEncode(request.Path));
			sb.Append('\n');
			sb.Append(querySign);
			sb.Append('\n');
			sb.Append("x-api-nonce:" + nonce);
			sb.Append('\n');
			sb.Append("x-api-timestamp:" + timestamp);
			sb.Append('\n');
			return SHAHelper.HmacSHA256(_clientSecret, sb.ToString());
		}

		private string GetSignature(string Method, string path, string queryString, string nonce, string timestamp)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Method.ToString().ToUpper());
			sb.Append('\n');
			sb.Append(PathEncode(path));
			sb.Append('\n');
			sb.Append(queryString);
			sb.Append('\n');
			sb.Append("x-api-nonce:" + nonce);
			sb.Append('\n');
			sb.Append("x-api-timestamp:" + timestamp);
			sb.Append('\n');
			return SHAHelper.HmacSHA256(_clientSecret, sb.ToString());
		}

		private string PostGetSignature(APIRequest request, string nonce, string timestamp)
		{
			var querySign = PostGetQueryString(request, true);

			StringBuilder sb = new StringBuilder();
			sb.Append(request.Method.ToString().ToUpper());
			sb.Append('\n');
			sb.Append(PathEncode(request.Path));
			sb.Append('\n');
			sb.Append(querySign);
			sb.Append('\n');
			sb.Append("x-api-nonce:" + nonce);
			sb.Append('\n');
			sb.Append("x-api-timestamp:" + timestamp);
			sb.Append('\n');
			return SHAHelper.HmacSHA256(_clientSecret, sb.ToString());
		}
		/// <summary>
		/// Url Path部分Url编码
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static string PathEncode(string path)
		{
			path = HttpUtility.UrlEncode("/" + path.TrimStart('/')).Replace("%2f", "%2F").Replace("%2d", "%2D").Replace("(", "%28").Replace(")", "%29");
			return path;
		}

		/// <summary>
		/// 查询字符串部分拼接
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private static string GetQueryString(APIRequest request, bool doubleEncode = false)
		{
			var querySign = string.Empty;
			if (request.Params != null && request.Params.Keys != null)
			{
				foreach (var key in request.Params.Keys)
				{
					if (doubleEncode)
					{
						querySign = querySign
										+ "&" + key.ToString() + "="
										+ WebUtility.UrlEncode(WebUtility.UrlEncode(request.Params[key])).Replace("+", "%20");//.ToUpper();
					}
					else
					{
						querySign = querySign
										+ "&" + key.ToString() + "="
										+ WebUtility.UrlEncode(request.Params[key]).Replace("+", "%20");//.ToUpper();
					}
				}
				querySign = querySign.TrimStart('&');
			}
			return querySign;
		}
		private static string GetQueryStringX(APIRequest request, bool doubleEncode = false)
		{
			var querySign = string.Empty;
			if (request.Params != null && request.Params.Keys != null)
			{
				foreach (var key in request.Params.Keys)
				{
					if (doubleEncode)
					{
						querySign = querySign
										+ "&" + key.ToString() + "="
										+ HttpUtility.UrlEncode(HttpUtility.UrlEncode(request.Params[key])).Replace("+", "%20").ToUpper(); ;//
					}
					else
					{
						querySign = querySign
										+ "&" + key.ToString() + "="
										+ HttpUtility.UrlEncode(request.Params[key]).Replace("+", "%20").ToUpper(); ;//.ToUpper();
					}
				}
				querySign = querySign.TrimStart('&');
			}
			return querySign;
		}

		private static string PostGetQueryString(APIRequest request, bool doubleEncode = false)
		{
			var querySign = string.Empty;
			if (request.Params != null && request.Params.Keys != null)
			{
				foreach (var key in request.Params.Keys)
				{
					if (doubleEncode)
					{
						querySign = querySign
										+ "&" + key.ToString() + "="
										+ WebUtility.UrlEncode(WebUtility.UrlEncode(request.Params[key])).Replace("+", "%20").ToUpper();
					}
					else
					{
						querySign = querySign
										+ "&" + key.ToString() + "="
										+ WebUtility.UrlEncode(request.Params[key]).Replace("+", "%20").ToUpper();
					}
				}
				querySign = querySign.TrimStart('&');
			}
			return querySign;
		}

	}
}