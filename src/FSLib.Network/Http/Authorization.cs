using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Net;

	/// <summary>
	/// 授权信息类
	/// </summary>
	public interface IAuthorization
	{
		/// <summary>
		/// 获得当前的HttpClient
		/// </summary>
		HttpClient HttpClient { get; set; }

		/// <summary>
		/// 将信息写入HttpWebRequest中
		/// </summary>
		/// <param name="request">请求</param>
		/// <param name="httpContext">当前的上下文</param>
		void SetRequest(HttpWebRequest request, HttpContext httpContext);
	}

	/// <summary>
	/// 授权信息类
	/// </summary>
	public abstract class Authorization : IAuthorization
	{
		/// <summary>
		/// 获得当前的HttpClient
		/// </summary>
		public HttpClient HttpClient { get; set; }

		/// <summary>
		/// 将信息写入HttpWebRequest中
		/// </summary>
		/// <param name="request">请求</param>
		/// <param name="httpContext">当前的上下文</param>
		public abstract void SetRequest(HttpWebRequest request, HttpContext httpContext);
	}
}
