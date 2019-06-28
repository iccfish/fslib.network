using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
using FSLib.Extension;

	/// <summary>
	/// HttpClient中出现的异常类
	/// </summary>
	public class HttpClientException : Exception
	{
		/// <summary>
		/// 创建新的 <see cref="HttpClientException"/> 对象
		/// </summary>
		/// <param name="msgName">错误消息名</param>
		/// <param name="args">格式化内容</param>
		public HttpClientException([NotNull]string msgName, params object[] args)
			: base(string.Format(SR.ResourceManager.GetString(msgName), args))
		{

		}
	}
}
