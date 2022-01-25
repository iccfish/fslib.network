using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	public class GlobalEvents
	{

		/// <summary>
		/// 准备发送请求
		/// </summary>
		public static event EventHandler<WebEventArgs> BeforeRequest;

		/// <summary>
		/// 获得对应的数据处理类
		/// </summary>
		public static event EventHandler<GetPreferredResponseTypeEventArgs> BeforeRequestGetPreferedResponseType;

		/// <summary>
		/// 请求将发送数据包装为请求承载数据
		/// </summary>
		public static event EventHandler<RequestWrapRequestContentEventArgs> BeforeRequestWrapRequestContent;
		/// <summary>
		/// WEB请求已取消
		/// </summary>
		public static event EventHandler<WebEventArgs> RequestCancelled;

		/// <summary>
		/// WEB请求结束
		/// </summary>
		public static event EventHandler<WebEventArgs> RequestEnd;


		/// <summary>
		/// Context已创建
		/// </summary>
		public static event EventHandler<WebEventArgs> HttpContextCreated;


		/// <summary>
		/// 请求发送失败
		/// </summary>
		public static event EventHandler<WebEventArgs> RequestFailed;

		/// <summary>
		/// 获得对应的数据处理类
		/// </summary>
		public static event EventHandler<GetPreferredResponseTypeEventArgs> RequestGetPreferedResponseType;


		/// <summary>
		/// WEB请求成功
		/// </summary>
		public static event EventHandler<WebEventArgs> RequestSuccess;

		/// <summary>
		/// 请求验证内容
		/// </summary>
		public static event EventHandler RequestValidateResponse;


		/// <summary>
		/// 请求将发送数据包装为请求承载数据
		/// </summary>
		public static event EventHandler<RequestWrapRequestContentEventArgs> RequestWrapRequestContent;



		/// <summary>
		/// 引发 <see cref="BeforeRequest" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		internal static void OnBeforeRequest(object sender, WebEventArgs ea)
		{
			var handler = BeforeRequest;

			if (handler != null)
				handler(sender, ea);
		}

		/// <summary>
		/// 引发 <see cref="RequestCancelled" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		internal static void OnRequestCancelled(object sender, WebEventArgs ea)
		{
			var handler = RequestCancelled;
			if (handler != null)
				handler(sender, ea);
		}

		/// <summary>
		/// 引发 <see cref="RequestEnd" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		internal static void OnRequestEnd(object sender, WebEventArgs ea)
		{
			var handler = RequestEnd;
			if (handler != null)
				handler(sender, ea);
		}


		/// <summary>
		/// 引发 <see cref="HttpContextCreated" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		internal static void OnHttpContextCreated(object sender, WebEventArgs ea)
		{
			var handler = HttpContextCreated;
			if (handler != null)
				handler(sender, ea);
		}

		/// <summary>
		/// 引发 <see cref="RequestFailed" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		internal static void OnRequestFailed(object sender, WebEventArgs ea)
		{
			var handler = RequestFailed;
			if (handler != null)
				handler(sender, ea);
		}

		/// <summary>
		/// 引发 <see cref="RequestSuccess" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		internal static void OnRequestSuccess(object sender, WebEventArgs ea)
		{
			var handler = RequestSuccess;
			if (handler != null)
				handler(sender, ea);
		}

		/// <summary>
		/// 引发 <see cref="BeforeRequestGetPreferedResponseType" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		public static void OnBeforeRequestGetPreferedResponseType(object sender, GetPreferredResponseTypeEventArgs ea)
		{
			var handler = BeforeRequestGetPreferedResponseType;
			if (handler != null)
				handler(sender, ea);
		}

		/// <summary>
		/// 引发 <see cref="BeforeRequestWrapRequestContent" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		public static void OnBeforeRequestWrapRequestContent(object sender, RequestWrapRequestContentEventArgs ea)
		{
			var handler = BeforeRequestWrapRequestContent;
			if (handler != null)
				handler(sender, ea);
		}


		/// <summary>
		/// 引发 <see cref="RequestGetPreferedResponseType" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		public static void OnRequestGetPreferedResponseType(object sender, GetPreferredResponseTypeEventArgs ea)
		{
			var handler = RequestGetPreferedResponseType;
			handler?.Invoke(sender, ea);
		}

		/// <summary>
		/// 引发 <see cref="RequestValidateResponse" /> 事件
		/// </summary>
		public static void OnRequestValidateResponse(object sender)
		{
			var handler = RequestValidateResponse;
			if (handler != null)
				handler(sender, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestWrapRequestContent" /> 事件
		/// </summary>
		/// <param name="sender">引发此事件的源对象</param>
		/// <param name="ea">包含此事件的参数</param>
		public static void OnRequestWrapRequestContent(object sender, RequestWrapRequestContentEventArgs ea)
		{
			var handler = RequestWrapRequestContent;
			if (handler != null)
				handler(sender, ea);
		}

	}
}
