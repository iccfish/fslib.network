using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace FSLib.Network.Http
{
	/// <summary>
	/// 额外的不常用的请求信息
	/// </summary>
	public class ExtraRequestInfo
	{
		/// <summary>
		/// 内容类型
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// 是否禁止302跳转。如果设置为 <see langword="true" />，则当服务器返回302/301跳转时，视为错误
		/// </summary>
		public bool Disable302Redirection { get; set; }

		/// <summary>
		/// 获得或设置如果请求发生了HTTP协议级别的错误（返回码大于400小于等于599），那么返回什么样的内容 
		/// </summary>
		/// <value>The response object.</value>
		/// <remarks>
		/// <para>此处设计用于当服务器返回禁止类型错误时，自动返回期望的结果。</para>
		/// <para>这通常用于在API接口等，对于40x错误有统一的返回结果，通常针对JSON处理</para>
		/// <para>因此，这里的设置仅针对40x代码且返回的是json时起效</para>
		/// </remarks>
		public ResponseObjectWrapper ErrorResponseObject { get; set; }
	}
}
