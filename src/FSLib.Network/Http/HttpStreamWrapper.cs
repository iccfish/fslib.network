using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Threading;
#if NET_GT_4
	using System.Threading.Tasks;
#endif

	/// <summary>
	/// HTTP数据流的封装
	/// </summary>
	public class HttpStreamWrapper : System.IO.Stream
	{
		/// <summary>
		/// 获得原始流
		/// </summary>
		public Stream BaseStream { get; private set; }

		/// <summary>
		/// 获得克隆的数据流
		/// </summary>
		public StreamWithEventsWrapper MirrorStream { get; private set; }

		private long _position, _streamLength;
		bool _enableClone;

		/// <summary>
		/// 设置是否允许克隆
		/// </summary>
		internal void EnableMirror()
		{
			_enableClone = true;

			if (MirrorStream == null)
				MirrorStream = new StreamWithEventsWrapper(new MemoryStream(Math.Max((int)_streamLength, 0x1000)));
		}

		/// <summary>
		/// 创建 <see cref="HttpStreamWrapper" />  的新实例(HttpReponseStreamWrapper)
		/// </summary>
		public HttpStreamWrapper(Stream baseStream, long streamLength)
		{
			if (baseStream == null)
				throw new ArgumentNullException("baseStream", "baseStream is null.");

			BaseStream = baseStream;
			_streamLength = streamLength;
			_position = 0L;
		}

		#region Overrides of Stream

		byte[] _prevReadBuffer;
		int _prevOffset;
		int _prevCount;

		/// <summary>
		/// 开始异步写操作。 （考虑使用<see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)"/>进行替换；请参见“备注”部分。）
		/// </summary>
		/// <returns>
		/// 表示异步写入的 IAsyncResult（可能仍处于挂起状态）。
		/// </returns>
		/// <param name="buffer">从中写入数据的缓冲区。</param><param name="offset"><paramref name="buffer"/> 中的字节偏移量，从此处开始写入。</param><param name="count">最多写入的字节数。</param><param name="callback">可选的异步回调，在完成写入时调用。</param><param name="state">一个用户提供的对象，它将该特定的异步写入请求与其他请求区别开来。</param><exception cref="T:System.IO.IOException">尝试进行的异步写入超过了流的结尾，或者发生了磁盘错误。</exception><exception cref="T:System.ArgumentException">一个或多个参数无效。</exception><exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception><exception cref="T:System.NotSupportedException">当前 Stream 实现不支持写入操作。</exception>
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (_enableClone)
			{
				MirrorStream.Write(buffer, offset, count);
			}
			return base.BeginWrite(buffer, offset, count, callback, state);
		}

		/// <summary>
		/// 开始异步读操作。 （考虑使用<see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)"/>进行替换；请参见“备注”部分。）
		/// </summary>
		/// <returns>
		/// 表示异步读取的 <see cref="T:System.IAsyncResult"/>（可能仍处于挂起状态）。
		/// </returns>
		/// <param name="buffer">数据读入的缓冲区。</param><param name="offset"><paramref name="buffer"/> 中的字节偏移量，从该偏移量开始写入从流中读取的数据。</param><param name="count">最多读取的字节数。</param><param name="callback">可选的异步回调，在完成读取时调用。</param><param name="state">一个用户提供的对象，它将该特定的异步读取请求与其他请求区别开来。</param><exception cref="T:System.IO.IOException">尝试的异步读取超过了流的结尾，或者发生了磁盘错误。</exception><exception cref="T:System.ArgumentException">一个或多个参数无效。</exception><exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception><exception cref="T:System.NotSupportedException">当前 Stream 实现不支持读取操作。</exception>
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			var result = base.BeginRead(buffer, offset, count, callback, state);

			if (_enableClone)
			{
				_prevCount = count;
				_prevReadBuffer = buffer;
				_prevOffset = offset;
			}

			return result;
		}

		/// <summary>
		/// 等待挂起的异步读取完成。
		/// </summary>
		/// <returns>
		/// 从流中读取的字节数，介于零 (0) 和所请求的字节数之间。流仅在流的末尾返回零 (0)，否则应一直阻止到至少有 1 个字节可用为止。
		/// </returns>
		/// <param name="asyncResult">对要完成的挂起异步请求的引用。</param><exception cref="T:System.ArgumentNullException"><paramref name="asyncResult"/> 为 null。</exception><exception cref="T:System.ArgumentException"><paramref name="asyncResult"/> 并非源自当前流上的 <see cref="M:System.IO.Stream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> 方法。</exception><exception cref="T:System.IO.IOException">此流关闭或发生内部错误。</exception>
		public override int EndRead(IAsyncResult asyncResult)
		{
			var count = base.EndRead(asyncResult);
			if (asyncResult.IsCompleted)
			{
				if (_enableClone)
				{
					MirrorStream.Write(_prevReadBuffer, _prevOffset, count);
					_prevReadBuffer = null;
					_prevOffset = 0;
					_prevCount = 0;
				}

			}
			return count;
		}

		public override void Flush()
		{
			BaseStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException();
		}

		public override void SetLength(long value)
		{
			throw new InvalidOperationException();
		}

		/// <summary>
		/// 当在派生类中重写时，从当前流读取字节序列，并将此流中的位置提升读取的字节数。
		/// </summary>
		/// <returns>
		/// 读入缓冲区中的总字节数。如果当前可用的字节数没有请求的字节数那么多，则总字节数可能小于请求的字节数；如果已到达流的末尾，则为零 (0)。
		/// </returns>
		/// <param name="buffer">字节数组。此方法返回时，该缓冲区包含指定的字符数组，该数组的 <paramref name="offset"/> 和 (<paramref name="offset"/> + <paramref name="count"/> -1) 之间的值由从当前源中读取的字节替换。</param><param name="offset"><paramref name="buffer"/> 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param><param name="count">要从当前流中最多读取的字节数。</param><exception cref="T:System.ArgumentException"><paramref name="offset"/> 与 <paramref name="count"/> 的和大于缓冲区长度。</exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> 为 null。</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> 或 <paramref name="count"/> 为负。</exception><exception cref="T:System.IO.IOException">发生 I/O 错误。</exception><exception cref="T:System.NotSupportedException">流不支持读取。</exception><exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception><filterpriority>1</filterpriority>
		public override int Read(byte[] buffer, int offset, int count)
		{
			var readCount = BaseStream.Read(buffer, 0, count);
			if (_enableClone)
				MirrorStream.Write(buffer, offset, readCount);
			_position += readCount;
			OnProgressChanged(new DataProgressEventArgs(_streamLength, _position));
			return readCount;
		}

		/// <summary>
		/// 当在派生类中重写时，向当前流中写入字节序列，并将此流中的当前位置提升写入的字节数。
		/// </summary>
		/// <param name="buffer">字节数组。此方法将 <paramref name="count"/> 个字节从 <paramref name="buffer"/> 复制到当前流。</param><param name="offset"><paramref name="buffer"/> 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param><param name="count">要写入当前流的字节数。</param><exception cref="T:System.ArgumentException"><paramref name="offset"/> 与 <paramref name="count"/> 的和大于缓冲区长度。</exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> 为 null。</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> 或 <paramref name="count"/> 为负。</exception><exception cref="T:System.IO.IOException">发生 I/O 错误。</exception><exception cref="T:System.NotSupportedException">流不支持写入。</exception><exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception><filterpriority>1</filterpriority>
		public override void Write(byte[] buffer, int offset, int count)
		{
			BaseStream.Write(buffer, 0, count);
			if (_enableClone)
				MirrorStream.Write(buffer, offset, count);
			_position += count;
			OnProgressChanged(new DataProgressEventArgs(_streamLength, _position));
		}

		/// <summary>
		/// 当在派生类中重写时，获取指示当前流是否支持读取的值。
		/// </summary>
		/// <returns>
		/// 如果流支持读取，为 true；否则为 false。
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public override bool CanRead
		{
			get { return BaseStream.CanRead; }
		}

		/// <summary>
		/// 当在派生类中重写时，获取指示当前流是否支持查找功能的值。
		/// </summary>
		/// <returns>
		/// 如果流支持查找，为 true；否则为 false。
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public override bool CanSeek
		{
			get { return false; }
		}

		/// <summary>
		/// 当在派生类中重写时，获取指示当前流是否支持写入功能的值。
		/// </summary>
		/// <returns>
		/// 如果流支持写入，为 true；否则为 false。
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public override bool CanWrite
		{
			get { return BaseStream.CanWrite; }
		}

		public override long Length
		{
			get { return _streamLength; }
		}

		public override long Position
		{
			get { return _position; }
			set { throw new InvalidOperationException(); }
		}

		#endregion

		/// <summary>
		/// 读取进度发生变化
		/// </summary>
		public event EventHandler<DataProgressEventArgs> ProgressChanged;

		/// <summary>
		/// 引发 <see cref="ProgressChanged" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnProgressChanged(DataProgressEventArgs ea = null)
		{
			if (ea == null)
				ea = new DataProgressEventArgs(_streamLength, _position);

			var handler = ProgressChanged;
			if (handler != null)
				handler(this, ea);
		}


#if NET45

		/// <summary>
		/// 从当前流异步读取字节序列，将流中的位置向前移动读取的字节数，并监控取消请求。
		/// </summary>
		/// <returns>
		/// 表示异步读取操作的任务。 
		/// </returns>
		/// <param name="buffer">数据写入的缓冲区。</param><param name="offset"><paramref name="buffer"/> 中的字节偏移量，从该偏移量开始写入从流中读取的数据。</param><param name="count">最多读取的字节数。</param><param name="cancellationToken">针对取消请求监视的标记。 默认值为 <see cref="P:System.Threading.CancellationToken.None"/>。</param><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> 为 null。</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> 或 <paramref name="count"/> 为负。</exception><exception cref="T:System.ArgumentException"><paramref name="offset"/> 与 <paramref name="count"/> 的和大于缓冲区长度。</exception><exception cref="T:System.NotSupportedException">流不支持读取。</exception><exception cref="T:System.ObjectDisposedException">流已被释放。</exception><exception cref="T:System.InvalidOperationException">该流正在由其前一次读取操作使用。</exception>
		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var readcount = await BaseStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
			if (_enableClone)
				await MirrorStream.WriteAsync(buffer, offset, readcount, cancellationToken).ConfigureAwait(true);
			_position += readcount;
			OnProgressChanged(new DataProgressEventArgs(_streamLength, _position));
			return readcount;
		}

		/// <summary>
		/// 将字节序列异步写入当前流，通过写入的字节数提前该流的当前位置，并监视取消请求数。
		/// </summary>
		/// <returns>
		/// 表示异步写入操作的任务。
		/// </returns>
		/// <param name="buffer">从中写入数据的缓冲区。</param><param name="offset"><paramref name="buffer"/> 中的从零开始的字节偏移量，从此处开始将字节复制到该流。</param><param name="count">最多写入的字节数。</param><param name="cancellationToken">针对取消请求监视的标记。 默认值为 <see cref="P:System.Threading.CancellationToken.None"/>。</param><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> 为 null。</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> 或 <paramref name="count"/> 为负。</exception><exception cref="T:System.ArgumentException"><paramref name="offset"/> 与 <paramref name="count"/> 的和大于缓冲区长度。</exception><exception cref="T:System.NotSupportedException">流不支持写入。</exception><exception cref="T:System.ObjectDisposedException">流已被释放。</exception><exception cref="T:System.InvalidOperationException">该流正在由其前一次写入操作使用。</exception>
		public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			await BaseStream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
			if (_enableClone)
				await MirrorStream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(true);
			_position += count;
			OnProgressChanged(new DataProgressEventArgs(_streamLength, _position));
		}

#endif

		/// <summary>
		/// 关闭当前流并释放与之关联的所有资源（如套接字和文件句柄）。
		/// </summary>
		public override void Close()
		{
			base.Close();
			BaseStream?.Close();
			MirrorStream?.Close();
		}
	}
}
