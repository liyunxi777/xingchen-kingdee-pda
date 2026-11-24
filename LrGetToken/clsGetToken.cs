using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.Threading.Tasks;
//using Kingdee.API.SDK;
using System.Net;
using System.Net.Http;
//using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LrGetToken
{
	public class clsGetToken
	{
		public async void GetAppSecret(string clientId, string clientSecret, string app_key, string outerInstanceId)
		{
			//var client = new APIClient("288026", "67ff1c3e744bed59c32c0dbc44a65648");
			//var apiRequest = new APIRequest();
			//apiRequest.Method = HttpMethod.Post;
			//apiRequest.Params.Add("outerInstanceId", "308290503223414784");
			//apiRequest.Path = "jdyconnector/app_management/push_app_authorize";
			//string strJson = client.HttpPost(apiRequest, clientId, "308290503223414784");
			//Console.WriteLine(strJson);
			//return strJson;
			var apiclient = new APIClient(clientId, clientSecret);
			APIRequest request = new APIRequest();
			request.Method = HttpMethod.Post;
			request.Params.Add("outerInstanceId", outerInstanceId);
			request.Path = "jdyconnector/app_management/push_app_authorize";
			var api_nonce = new Random().Next(100000000, 999999999);
			var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
			var responseMessage = await apiclient.Execute(request, api_nonce.ToString(), timeStamp.ToString());
			Console.WriteLine(responseMessage.Content.ReadAsStringAsync().Result);
		}

		public string GetAppSecretB(string clientId, string clientSecret, string app_key, string outerInstanceId)
		{
			var client = new APIClient(clientId, clientSecret);
			var apiRequest = new APIRequest();
			apiRequest.Method = HttpMethod.Post;
			apiRequest.Path = "jdyconnector/app_management/push_app_authorize";
			apiRequest.Params.Add("outerInstanceId", outerInstanceId);
			var api_nonce = new Random().Next(100000000, 999999999);
			var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;

			string strJson = client.HttpPost(apiRequest, clientId, outerInstanceId, api_nonce.ToString(), timeStamp.ToString());
			Console.WriteLine(strJson);
			return strJson;
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
				//var token = await client.Execute(apiRequest);
				//Console.WriteLine(token.Content.ReadAsStringAsync().Result);

				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.HttpGet(apiRequest, api_nonce.ToString(), timeStamp.ToString());
				Console.WriteLine(strJson);
				return strJson;
				//var apiclient = new APIClient(clientId, clientSecret);
				//APIRequest request = new APIRequest();
				//request.Method = HttpMethod.Get;
				//request.Params.Add("app_key", app_key);
				//request.Params.Add("app_signature", appsignature);
				//request.Path = "jdyconnector/app_management/kingdee_auth_token";
				//var responseMessage = await apiclient.Execute(request);
				//Console.WriteLine(responseMessage.Content.ReadAsStringAsync().Result);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}

		}

		public string SaveBusiBill(string clientId, string clientSecret, string appToken, string strPath, string strX)
		{
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Post;
				apiRequest.Path = strPath;
				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.SaveBill(apiRequest, clientId, appToken, api_nonce.ToString(), timeStamp.ToString(), strX);
				Console.WriteLine(strJson);
				return strJson;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return ex.Message;
			}
		}

		public string SaveBill(string clientId, string clientSecret, string appToken)
		{
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Post;
				apiRequest.Path = "jdy/v2/scm/inv_product";
				JObject jsonRoot = new JObject();
				jsonRoot.Add("bill_date", DateTime.Today.ToString("yyyy-MM-dd"));
				jsonRoot.Add("operation_key", "submit");
				JArray EntryRows = new JArray();
				jsonRoot.Add("material_entity", EntryRows);
				JObject LstRow = new JObject();
				//EntryRows.Add(LstRow);
				//LstRow.Add("material_id", "2005881943043876864");
				//LstRow.Add("qty", "1");
				//LstRow.Add("stock_id", "2012264939519019008");
				//LstRow.Add("sp_id", "2012265084121842688");
				//LstRow.Add("batch_no", "2022332131");
				//LstRow.Add("unit_id", "2012274958041940992");
				//LstRow.Add("unit_cost", "1200");

				LstRow = new JObject();
				EntryRows.Add(LstRow);
				LstRow.Add("material_id", "2021039527732250624");
				LstRow.Add("qty", "1000");
				LstRow.Add("stock_id", "2021047289283366912");
				LstRow.Add("batch_no", "20240820");
				LstRow.Add("kf_period", "1");
				LstRow.Add("kf_date", "2024-08-20");
				LstRow.Add("kf_type", "3");
				LstRow.Add("valid_date", "2025-08-19");
				LstRow.Add("unit_id", "6");
				LstRow.Add("src_bill_type_id", "mo_task_bill");
				LstRow.Add("src_entry_id", "2021125909288894465");
				LstRow.Add("src_inter_id", "2021125909288894464");
				string strTmp = "{\"bill_date\": \"2026-07-07\",\"remark\": \"2f25753f-08b7-10a4-f972-889d007be4c9\",\"operation_key\": \"submit\",\"material_entity\": [{\"material_id\": \"2509440949940245504\",\"qty\": 72.0,\"stock_id\": \"1838330468420813824\",\"sp_id\": \"1953678782825218048\",\"unit_id\": \"1838331956652236800\",\"src_bill_type_id\": \"mo_task_bill\",\"src_entry_id\": \"2512625497616185344\",\"src_inter_id\": \"2512625497616186368\",\"comment\": \"入库测试\"}]}"; 
				JsonConvert.SerializeObject(jsonRoot);
				string strX = jsonRoot.ToString();
				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.SaveBill(apiRequest, clientId, appToken, api_nonce.ToString(), timeStamp.ToString(), strTmp);
				Console.WriteLine(strJson);
				return strJson;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return "";
			}
		}

		public string UpdatePurBill(string clientId, string clientSecret, string appToken, string strData)
		{
			try
			{
				var client = new APIClient(clientId, clientSecret);
				var apiRequest = new APIRequest();
				apiRequest.Method = HttpMethod.Post;
				apiRequest.Path = "jdy/v2/scm/inv_product";

				string strTmp = "{\"bill_date\": \"2026-08-07\",\"id\": \"2540122700337239040\",\"material_entity\": [{\"custom_entity_field\": {\"custom_entity_field__1__6jiaxie8jvtv8pm6\": \"CGQC32020222\",\"custom_entity_field__2__6jiaxie8gdp716dyhx\": \"100\",\"custom_entity_field__2__6jiaxie8i6lltf8h\": \"98\",\"custom_entity_field__2__6jiaxie8jxiebd33fh\": \"2\"},\"delivery_date\": \"2026-08-07\",\"id\": \"2540122700337212416\",\"material_id\": \"2453837087967821824\",\"material_number\": \"BC-QT0001\",\"qty\": \"2026\",\"unit_id\": \"1\",\"unit_number\": \"个\"}],\"supplier_id\": \"2437934181808904192\",\"supplier_number\": \"GYS00001\"}";
 
				var api_nonce = new Random().Next(100000000, 999999999);
				var timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
				string strJson = client.SaveBill(apiRequest, clientId, appToken, api_nonce.ToString(), timeStamp.ToString(), strTmp);
				Console.WriteLine(strJson);
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