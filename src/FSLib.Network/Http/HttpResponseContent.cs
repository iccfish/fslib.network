using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FSLib.Network.Http
{
	using System.IO;

	/// <summary>
	/// 响应内容
	/// </summary>
	public abstract class HttpResponseContent : IDisposable
	{
		AsyncStreamProcessData _asyncData;
		protected Exception _exception;

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler PreContentProcessed;

		/// <summary>
		/// 引发 <see cref="PreContentProcessed"/> 事件
		/// </summary>

		protected virtual void OnPreContentProcessed()
		{
			PreContentProcessed?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler PostContentProcessed;

		/// <summary>
		/// 引发 <see cref="PostContentProcessed"/> 事件
		/// </summary>

		protected virtual void OnPostContentProcessed()
		{
			PostContentProcessed?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 创建 <see cref="HttpResponseContent" />  的新实例(HttpResponseContent)
		/// </summary>
		protected HttpResponseContent(HttpContext context, HttpClient client)
		{
			Context = context;
			Client = client;
		}

		/// <summary>
		/// 异步处理完成，调用回调
		/// </summary>
		protected void CompleteCallback()
		{
			Debug.Assert(_asyncData != null);

			_asyncData.NotifyAsyncComplete();
		}

		/// <summary>
		/// 请求初始化的最后时刻调用
		/// </summary>
		protected virtual void OnRequestInitInternal()
		{
		}

		/// <summary>
		/// 处理响应
		/// </summary>
		/// <param name="stream"></param>
		protected abstract void ProcessResponse(System.IO.Stream stream);

		/// <summary>
		/// 异步处理响应
		/// </summary>
		protected abstract void ProcessResponseAsync();

		/// <summary>
		/// 获得处理过程中的异步参数
		/// </summary>
		protected AsyncStreamProcessData AsyncData => _asyncData;

		/// <summary>
		/// 处理响应
		/// </summary>
		/// <param name="stream"></param>
		internal void InternalProcessResponse(System.IO.Stream stream)
		{
			ProcessResponse(stream);
		}

		/// <summary>
		/// 处理响应
		/// </summary>
		/// <param name="async">上下文数据</param>
		internal void InternalProcessResponseAsync(AsyncStreamProcessData async)
		{
			if (async == null)
				throw new ArgumentNullException(nameof(async), "async is null.");

			_asyncData = async;
			ProcessResponseAsync();
		}

		/// <summary>
		/// 初始化请求
		/// </summary>
		internal void OnRequestInit() => OnRequestInitInternal();

		/// <summary>
		/// 重置状态
		/// </summary>
		public virtual void Reset()
		{
			_exception = null;
		}

		/// <summary>
		/// 获得当前的请求客户端
		/// </summary>
		public HttpClient Client { get; private set; }

		/// <summary>
		/// 获得内容的实际长度（依据Header来）
		/// </summary>
		public virtual long ContentLength => Context.Response.ContentLength;

		/// <summary>
		/// 获得当前的上下文环境
		/// </summary>
		public HttpContext Context { get; private set; }
		/// <summary>
		/// 获得处理中发生的异常
		/// </summary>
		public Exception Exception
		{
			get
			{
				CheckDisposed();
				return _exception;
			}
			set { _exception = value; }
		}

		/// <summary>
		/// 获得响应的二进制内容。不是所有响应结果类型都可用
		/// </summary>
		public virtual byte[] RawBinaryData
		{
			get
			{
				throw new NotImplementedException("Get BinaryData operation not supported. ");
			}
			set
			{
				throw new NotImplementedException("Set BinaryData operation not supported. ");
			}
		}

		/// <summary>
		/// 获得原始响应流。不是所有响应结果类型都可用
		/// </summary>
		public virtual MemoryStream RawStream
		{
			get
			{
				throw new NotImplementedException("Get RawStream operation not supported. ");
			}
			set
			{
				throw new NotImplementedException("Set RawStream operation not supported. ");
			}
		}

		/// <summary>
		/// 获得响应的文本内容。不是所有响应结果类型都可用
		/// </summary>
		public virtual string RawStringResult
		{
			get
			{
				throw new NotImplementedException("Get StringResult operation not supported. ");
			}
			set
			{
				throw new NotImplementedException("Set StringResult operation not supported. ");
			}
		}

		/// <summary>
		/// 初始化以便于接收数据
		/// </summary>
		public virtual void Initialize()
		{
			
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

			}

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
