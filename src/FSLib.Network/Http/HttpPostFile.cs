using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.IO;

	/// <summary>
	/// 表示一个上传文件
	/// </summary>
	public class HttpPostFile : IDisposable
	{
		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		public HttpPostFile(string filePath) : this(null, filePath)
		{
		}

		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		public HttpPostFile(string fieldName, string filePath)
		{
			FieldName = fieldName;
			FilePath = filePath;
		}

		/// <summary>
		/// 数据写入进度变化
		/// </summary>
		public event EventHandler<DataProgressEventArgs> ProgressChanged;

		byte[] GetFieldHeaderBuffer()
		{
			var requestContent = Context.RequestContent as RequestFormDataContent;

			var str = "--" + requestContent.RequestBoundary + "\r\nContent-Disposition: form-data; name=\"" + FieldName + "\"; filename=\"" + _escapedFilePath + "\"\r\nContent-Type: " + ContentType + "\r\n\r\n";
			return Context.Request.Encoding.GetBytes(str);
		}

		/// <summary>
		/// 计算数据区长度
		/// </summary>
		/// <returns></returns>
		protected virtual long ComputeBodyLength()
		{
			FileInfo = new FileInfo(FilePath);
			return FileInfo.Length;
		}

		protected virtual long ComputeFooterLength()
		{
			return 2;
		}

		/// <summary>
		/// 计算开始信息长度
		/// </summary>
		/// <returns></returns>
		protected virtual long ComputeHeaderLength()
		{
			var requestContent = Context.RequestContent as RequestFormDataContent;
			//计算头
			return (long)2  //--
						+ requestContent.RequestBoundary.Length //boundary
						+ 2 //\r\n
						+ 32    // content-disposition
						+ 9
						+ FieldName.Length  //key
						+ 13
						+ Context.Request.Encoding.GetByteCount(_escapedFilePath)   //file name
						+ 14 + ContentType.Length    //content-type \r\n \r\n
						+ 2
						+ 2
						;
		}


		/// <summary>
		/// 引发 <see cref="ProgressChanged" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnProgressChanged(DataProgressEventArgs ea)
		{
			var handler = ProgressChanged;
			if (handler != null)
				handler(this, ea);
		}

		/// <summary>
		/// 写入数据区
		/// </summary>
		/// <param name="stream"></param>
		protected virtual void WriteBody(System.IO.Stream stream)
		{
			using (var fs = FileInfo.OpenRead())
			{
				var buffer = new byte[0x400 * 4];
				var count = 0;
				var pos = 0L;
				var length = fs.Length;
				var op = Context.Operation;

				var ee = new DataProgressEventArgs(length, 0L);
				if (op == null)
					OnProgressChanged(ee);
				else
					op.Post(_ => OnProgressChanged(ee), null);

				while ((count = fs.Read(buffer, 0, buffer.Length)) > 0)
				{
					stream.Write(buffer, 0, count);
					pos += count;
					ee = new DataProgressEventArgs(length, pos);

					if (op == null)
						OnProgressChanged(ee);
					else
						op.Post(_ => OnProgressChanged(ee), null);
				}
				fs.Close();
			}
		}

		/// <summary>
		/// 写入头信息
		/// </summary>
		/// <param name="stream"></param>
		protected virtual void WriteHeader(System.IO.Stream stream)
		{
			stream.Write(GetFieldHeaderBuffer());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		protected virtual void WriterFooter(System.IO.Stream stream)
		{
			stream.Write(System.Text.Encoding.ASCII.GetBytes("\r\n"));
		}

		protected virtual AsyncStreamProcessData AsyncData
		{
			get { return _asyncData; }
		}

		/// <summary>
		/// 绑定上下文
		/// </summary>
		/// <param name="context"></param>
		public virtual void AttachContext(HttpContext context)
		{
			Context = context;
		}

		/// <summary>
		/// 计算长度(含开始信息)
		/// </summary>
		/// <returns></returns>
		public virtual long ComputeLength()
		{
			if (_escapedFilePath == null && !FilePath.IsNullOrEmpty())
			{
				_escapedFilePath = System.Web.HttpUtility.UrlEncode(Path.GetFileName(FilePath), Context.Request.Encoding);
			}

			return ComputeHeaderLength() + ComputeBodyLength() + ComputeFooterLength();
		}

		/// <summary>
		/// 写入指定的流
		/// </summary>
		/// <param name="stream"></param>
		public virtual void WriteTo(System.IO.Stream stream)
		{
			WriteHeader(stream);
			WriteBody(stream);
			WriterFooter(stream);
		}

		/// <summary>
		/// Content-Type
		/// </summary>
		public string ContentType { get; set; } = "application/octet-stream";

		/// <summary>
		/// 获得上下文环境
		/// </summary>
		public HttpContext Context { get; private set; }
		/// <summary>
		/// 获得或设置表单名
		/// </summary>

		public string FieldName { get; set; }


		/// <summary>
		/// 获得或设置文件信息
		/// </summary>
		public FileInfo FileInfo { get; set; }

		private string _filePath;
		private string _escapedFilePath;

		/// <summary>
		/// 文件信息
		/// </summary>
		public string FilePath
		{
			get { return _filePath; }
			set
			{
				_filePath = value;
				_escapedFilePath = null;
			}
		}

		#region 异步模型

		byte[] _buffer;
		FileStream _fs;
		Action _callback;
		AsyncStreamProcessData _asyncData;

		public virtual void WriteToAsync(AsyncStreamProcessData asyncData, Action callback)
		{
			_asyncData = asyncData;
			_callback = callback;
			WriteHeaderAsync();
		}

		/// <summary>
		/// 写入头信息
		/// </summary>
		protected virtual void WriteHeaderAsync()
		{
			AsyncData.AsyncStreamWrite(GetFieldHeaderBuffer(), true, _ => WriteBodyAsync());
		}

		/// <summary>
		/// 写入头信息
		/// </summary>
		protected virtual void WriteFooterAsync()
		{
			AsyncData.AsyncStreamWrite(System.Text.Encoding.ASCII.GetBytes("\r\n"), true, _ =>
			{
				if (AsyncData.Exception == null)
					_callback();
			});
		}

		protected virtual void WriteBodyAsync()
		{
			var op = Context.Operation;
			//一个文件失败，则把整个过程看作失败
			try
			{
				_fs = FileInfo.OpenRead();
				_buffer = new byte[AsyncData.HttpContext.Client.Setting.WriteBufferSize];
				var ee = new DataProgressEventArgs(_fs.Length, 0L);
				if (op == null)
					OnProgressChanged(ee);
				else
					op.Post(__ => OnProgressChanged(ee), null);
				ReadFileAsync();
			}
			catch (Exception ex)
			{
				AsyncData.Exception = ex;
				AsyncData.NotifyAsyncComplete();
			}
		}

		void ReadFileAsync()
		{
			_fs.BeginRead(_buffer, 0, _buffer.Length, _ =>
			{
				try
				{
					var count = _fs.EndRead(_);
					var ee = new DataProgressEventArgs(_fs.Length, _fs.Position);
					var op = Context.Operation;

					if (op == null)
						OnProgressChanged(ee);
					else
						op.Post(__ => OnProgressChanged(ee), null);
					if (count == 0)
					{
						_fs.Close();
						WriteFooterAsync();
					}
					else
					{
						AsyncData.AsyncStreamWrite(_buffer, 0, count, true, __ => ReadFileAsync());
					}
				}
				catch (Exception ex)
				{
					AsyncData.Exception = ex;
					AsyncData.NotifyAsyncComplete();
				}
			}, null);

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
				if (_fs != null)
				{
					_fs.Dispose();
					_fs = null;
				}

			}

			//挂起终结器
			GC.SuppressFinalize(this);
		}

		#endregion

		#endregion
	}
}
