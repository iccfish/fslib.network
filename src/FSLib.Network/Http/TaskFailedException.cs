using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Net;

#if NET_GT_4

	/// <summary>
	/// 请求操作完成，但是没有能成功的异常
	/// </summary>
	public class TaskFailedException : Exception
	{
		/// <summary>
		/// 请求上下文
		/// </summary>
		public HttpContext Context { get; private set; }

		/// <summary>
		/// 请求信息
		/// </summary>
		public HttpRequestMessage RequestMessage { get; private set; }

		/// <summary>
		/// 响应内容
		/// </summary>
		public HttpResponseContent ResponseContent { get; private set; }

		/// <summary>
		/// 响应内容
		/// </summary>
		public HttpResponseMessage ResponseMessage { get; private set; }

		/// <summary>
		/// 请求内容
		/// </summary>
		public HttpRequestContent RequestContent { get; private set; }

		/// <summary>
		/// 状态码
		/// </summary>
		public HttpStatusCode? StatusCode { get; private set; }

		/// <summary>
		/// 请求操作完成，但是没有能成功的异常
		/// </summary>
		private TaskFailedException()
			: base(SR.httpexception_notsucceed)
		{
			
		}

		/// <summary>
		/// 创建新的错误对象
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		internal static TaskFailedException Create(HttpContext context)
		{
			var ex = new TaskFailedException();
			ex.Context = context;
			ex.RequestContent = context.RequestContent;
			ex.RequestMessage = context.Request;
			ex.ResponseContent = context.ResponseContent;
			ex.ResponseMessage = context.Response;
			if (ex.ResponseMessage != null)
				ex.StatusCode = ex.ResponseMessage.Status;

			return ex;
		}

	}

#endif
}
