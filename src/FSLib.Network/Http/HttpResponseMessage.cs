using System;
using System.Net;

namespace FSLib.Network.Http
{

	/// <summary>
	/// HTTP响应信息
	/// </summary>
	public class HttpResponseMessage : IDisposable
	{
		WwwAuthenticate _wwwAuthenticate;

		/// <summary>
		/// 获得响应内容
		/// </summary>
		public HttpWebResponse WebResponse { get; private set; }

		/// <summary>
		/// 创建 <see cref="HttpResponseMessage" />  的新实例(HttpResponseMessage)
		/// </summary>
		public HttpResponseMessage(HttpWebResponse webResponse)
		{
			WebResponse = webResponse;
			Cookies = WebResponse.Cookies;
			Status = WebResponse.StatusCode;
			StatusDescription = WebResponse.StatusDescription;
			ContentLength = WebResponse.ContentLength;
			ContentType = WebResponse.ContentType;
			ContentEncoding = WebResponse.ContentEncoding;
			Headers = WebResponse.Headers;
			CharacterSet = WebResponse.CharacterSet;
			HttpVersion = WebResponse.ProtocolVersion;
			Server = WebResponse.Server;
			Method = (HttpMethod)Enum.Parse(typeof(HttpMethod), webResponse.Method, true);
			ResponseUri = webResponse.ResponseUri;

			if (IsPartialContent)
			{
				ContentRange = new Range(Headers["Content-Range"]);
			}

			if (!string.IsNullOrEmpty(Headers[HttpResponseHeader.Date]))
				Date = Headers[HttpResponseHeader.Date].ToDateTimeNullable();
			CheckResponseState();
		}

		/// <summary>
		/// 获得当前响应解压缩模式
		/// </summary>
		public DecompressionMethods DecompressionMethod { get; internal set; }

		/// <summary>
		/// 检测响应状态
		/// </summary>
		void CheckResponseState()
		{
			var cacheStr = Headers["X-Cache"];
			var age = Headers["Age"];
			IsCachedByCdn = ((!cacheStr.IsNullOrEmpty() && cacheStr.IndexOf("HIT") != -1) || !age.IsNullOrEmpty());
		}

		/// <summary>
		/// 获得响应的时间
		/// </summary>
		public DateTime? Date { get; private set; }

		/// <summary>
		/// 获得响应是否是CDN的缓存
		/// </summary>
		public bool IsCachedByCdn { get; internal set; }

		DateTime? _lastModified;

		/// <summary>
		/// 获得最后修改
		/// </summary>
		public DateTime? LastModified
		{
			get
			{
				if (_lastModified == null)
				{
					try
					{
						_lastModified = WebResponse.LastModified;
					}
					catch (Exception)
					{

					}
				}

				return _lastModified;
			}
		}

		/// <summary>
		/// 获得HTTP版本
		/// </summary>
		public Version HttpVersion { get; internal set; }

		/// <summary>
		/// 获得服务器标头
		/// </summary>
		public string Server { get; internal set; }

		/// <summary>
		/// 获得响应的方法
		/// </summary>
		public HttpMethod Method { get; internal set; }

		/// <summary>
		/// 获得响应的最终地址
		/// </summary>
		public Uri ResponseUri { get; internal set; }

		/// <summary>
		/// 获得响应的字符集
		/// </summary>
		public string CharacterSet { get; private set; }

		/// <summary>
		/// 获得响应头
		/// </summary>
		public WebHeaderCollection Headers { get; private set; }

		/// <summary>
		/// 获得响应的Cookies
		/// </summary>
		public CookieCollection Cookies
		{
			get;
			private set;
		}

		/// <summary>
		/// 获得状态码
		/// </summary>
		public HttpStatusCode Status
		{
			get;
			private set;
		}

		/// <summary>
		/// 获得状态码
		/// </summary>
		public string StatusDescription
		{
			get;
			private set;
		}

		/// <summary>
		/// 获得响应的内容
		/// </summary>
		public long ContentLength
		{
			get;
			private set;
		}

		/// <summary>
		/// 获得响应类型
		/// </summary>
		public string ContentType
		{
			get;
			private set;
		}

		/// <summary>
		/// 获得当前的重定向信息
		/// </summary>
		public HttpRedirection Redirection { get; internal set; }

		/// <summary>
		/// 获得响应编码
		/// </summary>
		public string ContentEncoding
		{
			get;
			private set;
		}

		/// <summary>
		/// 获得接受的域
		/// </summary>
		public string AcceptRange
		{
			get { return Headers[HttpResponseHeader.AcceptRanges]; }
		}

		/// <summary>
		/// 获得当前包含的响应区域
		/// </summary>
		public Range ContentRange { get; private set; }

		/// <summary>
		/// 获得是否是部分响应
		/// </summary>
		public bool IsPartialContent
		{
			get { return Status == HttpStatusCode.PartialContent; }
		}

		/// <summary>
		/// 获得响应标头中的地址
		/// </summary>
		public string Location
		{
			get { return Headers[HttpResponseHeader.Location]; }
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static implicit operator HttpWebResponse(HttpResponseMessage message)
		{
			return message.WebResponse;
		}

		/// <summary>
		/// 获得实际的内容
		/// </summary>
		public HttpResponseContent Content { get; internal set; }

		/// <summary>
		/// 获得请求中的身份验证标记
		/// </summary>
		public WwwAuthenticate WwwAuthenticate
		{
			get
			{
				if (_wwwAuthenticate == null)
				{
					var header = Headers[HttpResponseHeader.WwwAuthenticate];
					if (!string.IsNullOrEmpty(header))
					{
						_wwwAuthenticate = new WwwAuthenticate(header);
					}
				}
				return _wwwAuthenticate;
			}

		}

		#region Dispose方法实现

		bool _disposed;

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			_disposed = true;

			if (disposing)
			{
				if (Content != null)
					Content.Dispose();
			}
			//TODO 释放非托管资源

			//挂起终结器
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 检查是否已经被销毁。如果被销毁，则抛出异常
		/// </summary>
		/// <exception cref="ObjectDisposedException">对象已被销毁</exception>
		protected void CheckDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(this.GetType().Name);
		}


		#endregion

	}
}
