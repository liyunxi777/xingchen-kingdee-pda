using System.Collections.Generic;
using System.Net.Http;

namespace LrGetToken
{
	/// <summary>
	/// API请求类
	/// </summary>
	public class APIRequest
	{
		private string _path = "";

		/// <summary>
		/// 请求路径
		/// </summary>
		public string Path
		{
			get
			{
				return _path.TrimStart('/').TrimEnd('/');
			}
			set
			{
				_path = value;
			}
		}

		///// <summary>
		///// 请求Method
		///// </summary>
		public HttpMethod Method = HttpMethod.Get;

		///// <summary>
		///// 请求ContentType，默认为 application/json
		///// </summary>
		public string ContentType = "application/json";

		/// <summary>
		/// Param 参数
		/// </summary>
		public IDictionary<string, string> Params = new Dictionary<string, string>();

		/// <summary>
		/// 附加的Http Header
		/// </summary>
		public IDictionary<string, string> Header = new Dictionary<string, string>();

		/// <summary>
		/// 请求体
		/// </summary>
		public object Body { get; set; }

		/// <summary>
		/// 上传文件的路径
		/// </summary>
		public string File { get; set; }
	}
}