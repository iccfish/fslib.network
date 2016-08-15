using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 请求的内容类型
	/// </summary>
	public enum ContentType
	{
		/// <summary>
		/// 无
		/// </summary>
		None,
		/// <summary>
		/// 纯文本
		/// </summary>
		PlainText,
		/// <summary>
		/// 二进制
		/// </summary>
		Binary,
		/// <summary>
		/// Application/Json
		/// </summary>
		Json,
		/// <summary>
		/// Application/Javascript
		/// </summary>
		Javascript,
		/// <summary>
		/// text/xml
		/// </summary>
		Xml,
		/// <summary>
		/// application/xml
		/// </summary>
		XmlApp,
		/// <summary>
		/// text/html
		/// </summary>
		Html,
		/// <summary>
		/// multipart/form-data
		/// </summary>
		FormData,
		/// <summary>
		/// application/x-www-form-urlencoded
		/// </summary>
		FormUrlEncoded
	}
}
