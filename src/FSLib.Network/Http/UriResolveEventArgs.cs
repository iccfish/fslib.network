using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Net;

	/// <summary>
	/// 表示将要进行解析的URL参数
	/// </summary>
	public class UriResolveEventArgs : EventArgs
	{
		/// <summary>
		/// 获得将要解析的源地址
		/// </summary>
		public string Url { get; private set; }

		/// <summary>
		/// 获得或设置解析后的Uri
		/// </summary>
		public Uri Uri { get; set; }

		/// <summary>
		/// 获得与此相关的请求头
		/// </summary>
		public HttpRequestHeader? RequestHeader { get; private set; }


		/// <summary>
		/// 获得关联的上下文数据
		/// </summary>
		public Dictionary<string, object> ContextData { get; private set; }

		/// <summary>
		/// 创建 <see cref="UriResolveEventArgs" />  的新实例(UriResolveEventArgs)
		/// </summary>
		/// <param name="url"></param>
		/// <param name="requestHeader"></param>
		/// <param name="contextData"></param>
		public UriResolveEventArgs(string url, HttpRequestHeader? requestHeader, Dictionary<string, object> contextData)
		{
			Url = url;
			RequestHeader = requestHeader;
			ContextData = contextData;
		}
	}
}
