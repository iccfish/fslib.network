using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FSLib.Network.Http
{
	using System.IO;
	using System.Net;

	/// <summary>
	/// HTTP请求内容
	/// </summary>
	public abstract class HttpRequestContent : IDisposable
	{
		/// <summary>
		/// 获得客户端已知的支持发送的数据类型
		/// </summary>
		public static HashSet<string> KnownContentTypes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"text/plain",
			"application/json",
			"application/javascript",
			"text/xml",
			"application/xml",
			"text/html",
			"multipart/form-data",
			"application/x-www-form-urlencoded"
		};

		/// <summary>
		/// 创建 <see cref="HttpRequestContent" />  的新实例(HttpRequestContent)
		/// </summary>
		/// <param name="contentType"></param>
		/// <param name="contentTypeString"></param>
		protected HttpRequestContent(ContentType contentType = ContentType.FormUrlEncoded, string contentTypeString = null)
		{
			ContentType = contentType == ContentType.None ? ContentType.FormUrlEncoded : contentType;
			ContentTypeString = contentTypeString;
		}

		/// <summary>
		/// 将字符串隐式转换为内容
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static implicit operator HttpRequestContent(string content)
		{
			return (RequestStringContent)content;
		}

		/// <summary>
		/// 将字符串隐式转换为内容
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static implicit operator HttpRequestContent(byte[] content)
		{
			return (RequestByteBufferContent)content;
		}

		/// <summary>
		/// 将流转换为请求内容
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static implicit operator HttpRequestContent(Stream stream)
		{
			return (RequestCopyStreamContent)stream;
		}

		/// <summary>
		/// 通知异步的操作完成
		/// </summary>
		protected virtual void CallCompleteCallback()
		{
			if (AsyncData != null)
				AsyncData.NotifyAsyncComplete();
		}

		/// <summary>
		/// 获得在请求发送过程中的数据
		/// </summary>
		protected AsyncStreamProcessData AsyncData { get; private set; }

		/// <summary>
		/// 获得上下文环境
		/// </summary>
		protected HttpContext Context
		{
			get;
			set;
		}

		/// <summary>
		/// 获得请求消息
		/// </summary>
		protected HttpRequestMessage Message { get; private set; }

		/// <summary>
		/// 获得上下文请求
		/// </summary>
		protected HttpWebRequest WebRequset { get; private set; }

		/// <summary>
		/// 绑定上下文环境
		/// </summary>
		/// <param name="context"></param>
		/// <param name="message"></param>
		/// <param name="client"></param>
		public virtual void BindContext(HttpClient client, HttpContext context, HttpRequestMessage message)
		{
			Client = client;
			Context = context;
			Message = message;
		}

		/// <summary>
		/// 计算长度
		/// </summary>
		/// <returns></returns>
		public abstract long ComputeLength();

		/// <summary>
		/// 准备发出请求
		/// </summary>
		/// <param name="request"></param>
		public virtual void Prepare(HttpWebRequest request)
		{
			WebRequset = request;

			var pt = ContentType;
			var ct = "";

			switch (pt)
			{
				case ContentType.None:
				case ContentType.Binary:
					ct = ContentTypeString.DefaultForEmpty("application/octet-stream");
					break;
				case ContentType.PlainText:
					ct = "text/plain";
					break;
				case ContentType.Json:
					ct = "application/json";
					break;
				case ContentType.Javascript:
					ct = "application/javascript";
					break;
				case ContentType.Xml:
					ct = "text/xml";
					break;
				case ContentType.XmlApp:
					ct = "application/xml";
					break;
				case ContentType.Html:
					ct = "text/html";
					break;
				case ContentType.FormData:
					ct = "multipart/form-data";
					break;
				case ContentType.FormUrlEncoded:
					ct = "application/x-www-form-urlencoded";
					break;
			}
			if (Client.Setting.AutoAppendCharsetInContentType)
			{
				ct += "; charset=" + Context.Request.Encoding.WebName;
			}
			request.ContentType = ct;


			if (Message.AllowRequestBody)
			{
				var contentLength = ContentLength = ComputeLength();
				//如果长度为-1，则需要对写数据进行缓冲，否则会引发异常
				if (request.ContentLength < 0)
					request.AllowWriteStreamBuffering = true;
				else
					request.ContentLength = contentLength;
				Context.Performance.RequestLength = contentLength;
			}
		}

		/// <summary>
		/// 准备数据
		/// </summary>
		public virtual void PrepareData()
		{

		}

		/// <summary>
		/// 将当前的内容序列化到查询中
		/// </summary>
		public virtual string SerializeAsQueryString()
		{
			throw new NotSupportedException("当前的查询内容无法转换为查询字符串");
		}

		/// <summary>
		/// 变更发出响应的ContentType类型
		/// </summary>
		/// <param name="contentType"></param>
		/// <exception cref="ArgumentException"></exception>
		public void SetContentType(string contentType)
		{
			if (string.IsNullOrEmpty(contentType))
				throw new ArgumentException($"{nameof(contentType)} is null or empty.", nameof(contentType));

			ContentType = ContentType.None;
			ContentTypeString = contentType;
		}

		/// <summary>
		/// 写入内容到流中
		/// </summary>
		/// <param name="stream"></param>
		public abstract void WriteTo(Stream stream);

		/// <summary>
		/// 异步将数据写入当前的请求流中
		/// </summary>
		/// <param name="asyncData"></param>
		public virtual void WriteToAsync(AsyncStreamProcessData asyncData)
		{
			AsyncData = asyncData;
		}


		/// <summary>
		/// 获得客户端
		/// </summary>
		public HttpClient Client { get; private set; }

		/// <summary>
		/// 获得计算后的内容长度
		/// </summary>
		public virtual long ContentLength { get; private set; }


		/// <summary>
		/// 获得或设置请求数据的发送方式
		/// </summary>
		public ContentType ContentType { get; set; }

		private string _contentTypeString;

		/// <summary>
		/// 获得或设置自定义数据类型
		/// </summary>
		public string ContentTypeString
		{
			get { return _contentTypeString; }
			set
			{
				if (ContentType != ContentType.None)
					return;

				if(value.IsNullOrEmpty())
					throw new ArgumentException(SR.HttpRequestContent_ContentTypeString_NullOrEmpty, nameof(value));

				var ct = value;

				//去掉字符集
				var charIndex = ct.IndexOf(';');
				if (charIndex != -1)
				{
					ct = ct.Substring(0, charIndex);
				}

				ct = ct.Trim();
				if (KnownContentTypes.Contains(ct))
				{
					throw new ArgumentException(SR.HttpRequestContent_ContentTypeString_ShouldUseProperty);
				}

				_contentTypeString = ct;
			}
		}

		#region Dispose方法实现

		/// <summary>
		/// 检查是否已经被销毁。如果被销毁，则抛出异常
		/// </summary>
		/// <exception cref="ObjectDisposedException">对象已被销毁</exception>
		protected void CheckDisposed()
		{
			if (Disposed)
				throw new ObjectDisposedException(this.GetType().Name);
		}


		/// <summary>
		/// 获得当前的对象是否已经被销毁
		/// </summary>
		protected bool Disposed { get; private set; }

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 销毁当前对象
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (Disposed) return;
			Disposed = true;

			if (disposing)
			{
			}

			//挂起终结器
			GC.SuppressFinalize(this);
		}

		#endregion


	}
}
