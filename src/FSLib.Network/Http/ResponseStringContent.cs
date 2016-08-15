using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 表示字符串响应
	/// </summary>
	public class ResponseStringContent : ResponseBinaryContent
	{
		/// <summary>
		/// 创建 <see cref="ResponseStringContent"/>  的新实例(ResponseStringContent)
		/// </summary>
		public ResponseStringContent(HttpContext context, HttpClient client)
			: base(context, client)
		{

		}

		/// <summary>
		/// 获得响应内容
		/// </summary>
		public new string Result { get { return base.StringResult; } }

		/// <summary>
		/// 返回表示当前对象的字符串。
		/// </summary>
		/// <returns>
		/// 表示当前对象的字符串。
		/// </returns>
		public override string ToString()
		{
			return Result;
		}
	}
}
