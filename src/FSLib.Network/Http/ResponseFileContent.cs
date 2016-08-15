using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Diagnostics;
	using System.FishLib;
	using System.Net;

	/// <summary>
	/// 保存为文件的响应
	/// </summary>
	public class ResponseFileContent : HttpResponseContent, IDisposable
	{
		/// <summary>
		/// 本次下载之前就已经下载的长度
		/// </summary>
		public long PreDownloadDataLength { get; set; }

		/// <summary>
		/// 获得或设置要保存到的文件
		/// </summary>
		public string FilePath { get; set; }

		/// <summary>
		/// 获得是否保存成功
		/// </summary>
		public bool Success { get; protected set; }

		/// <summary>
		/// 获得或设置使用启用断点续传
		/// </summary>
		public bool EnableRestartProcess { get; set; } = true;

		/// <summary>
		/// 创建 <see cref="HttpResponseContent" />  的新实例(HttpResponseContent)
		/// </summary>
		internal ResponseFileContent(HttpContext context, HttpClient client, string filePath)
			: base(context, client)
		{
			FilePath = filePath;
		}

		/// <summary>
		/// 请求目标写入流
		/// </summary>
		public event EventHandler<GeneralEventArgs<Stream>> OpenWriteStream;

		#region Overrides of HttpResponseContent

		/// <summary>
		/// 请求初始化的最后时刻调用
		/// </summary>
		protected override void OnRequestInitInternal()
		{
			base.OnRequestInitInternal();

			//断点续传？
			if (EnableRestartProcess)
			{
				TrySetRangeFromFile();
			}
		}

		void TrySetRangeFromFile()
		{
			var fileinfo = new FileInfo(FilePath);
			if (!fileinfo.Exists)
				return;

			PreDownloadDataLength = fileinfo.Length;
#if NET_GT_4
			Context.Request.Range = new KeyValuePair<long, long?>(fileinfo.Length, null);
			Context.WebRequest.AddRange(fileinfo.Length);
#else
			Context.Request.Range = new KeyValuePair<int, int?>((int)fileinfo.Length, null);
			Context.WebRequest.AddRange((int)fileinfo.Length);
#endif
		}

		/// <summary>
		/// 处理响应
		/// </summary>
		/// <param name="stream"></param>
		protected override void ProcessResponse(Stream stream)
		{
			var fs = GetWriteStream();

			if (fs == null)
			{
				return;
			}

			try
			{
				var buffer = new byte[0x400 * 4];
				var count = 0;
				while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					fs.Write(buffer, 0, count);
				}
				Success = true;
			}
			finally
			{
				fs.Close();
			}
		}

		Stream _fileStream;
		byte[] _buffer;

		/// <summary>
		/// 异步处理响应
		/// </summary>
		protected override void ProcessResponseAsync()
		{
			try
			{
				_fileStream = GetWriteStream();
			}
			catch (Exception ex)
			{
				AsyncData.Exception = ex;
				CompleteCallback();
			}
			if (_fileStream == null)
				return;

			_buffer = new byte[AsyncData.HttpContext.Client.Setting.ReadBufferSize];
			AsyncData.Stream.BeginRead(_buffer, 0, _buffer.Length, NetworkReadCallback, this);
		}

		void NetworkReadCallback(IAsyncResult ar)
		{
			var count = 0;
			try
			{
				count = AsyncData.Stream.EndRead(ar);
			}
			catch (Exception exception)
			{
				_fileStream?.Close();
				AsyncData.Exception = exception;
				CompleteCallback();

				return;
			}

			if (count == 0)
			{
				_fileStream?.Close();
				Success = true;
				CompleteCallback();
			}
			else
			{
				_fileStream.BeginWrite(_buffer, 0, count, _ =>
				{
					try
					{
						_fileStream.EndWrite(_);
						AsyncData.Stream.BeginRead(_buffer, 0, _buffer.Length, NetworkReadCallback, this);
					}
					catch (Exception ex)
					{
						_fileStream?.Close();
						AsyncData.Exception = ex;
						CompleteCallback();
					}
				}, this);
			}
		}

		#endregion

		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_fileStream != null)
					_fileStream.Dispose();

			}
		}

		/// <summary>
		/// 打开写入的目标流
		/// </summary>
		/// <returns></returns>
		protected virtual Stream GetWriteStream()
		{
			Stream stream = null;

			try
			{
				var e = new GeneralEventArgs<Stream>(null);
				OnOpenWriteStream(e);

				stream = e.Data;
			}
			catch (Exception ex)
			{
				AsyncData.Exception = ex;
				CompleteCallback();
			}

			if (stream == null && !Success)
			{
				AsyncData.Exception = new Exception("Open target stream failed.");
				CompleteCallback();
			}

			return stream;
		}

		protected virtual void OnOpenWriteStream(GeneralEventArgs<Stream> e)
		{
			OpenWriteStream?.Invoke(this, e);

			if (e.Data == null)
			{
				System.IO.Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

				var fileinfo = new FileInfo(FilePath);
				if (fileinfo.Exists)
				{
					if (Context.Response.Status != HttpStatusCode.PartialContent)
					{
						EnableRestartProcess = false;
					}
					if (Context.Response.Status == HttpStatusCode.RequestedRangeNotSatisfiable)
					{
						//ok
						Success = true;
						return;
					}
				}

				e.Data = new System.IO.FileStream(FilePath, EnableRestartProcess ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None);
				if (!EnableRestartProcess)
				{
					PreDownloadDataLength = 0;
				}
			}
		}
	}
}
