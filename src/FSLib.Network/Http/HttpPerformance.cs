using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NET_GT_4
using System.Threading.Tasks;
#endif

namespace FSLib.Network.Http
{
	using System.Timers;

	/// <summary>
	/// 当前HTTP请求性能相关的记录
	/// </summary>
	public class HttpPerformance : IDisposable
	{
		HttpContext _context;
		Timer _speedTimer;
		long _startTicks, _lastTicks;
		long _dataProcessLast, _dataProcessed, _dataLength;
		bool _inUpload = true;

		/// <summary>
		/// 请求长度
		/// </summary>
		public long RequestLength { get; internal set; }

		/// <summary>
		/// 请求已发送长度
		/// </summary>
		public long RequestLengthSended { get; internal set; }

		/// <summary>
		/// 响应长度
		/// </summary>
		public long ResponseLength { get; internal set; }

		/// <summary>
		/// 响应已发送长度
		/// </summary>
		public long ResponseLengthProcessed { get; internal set; }

		/// <summary>
		/// 创建 <see cref="HttpPerformance"/> 的新实例
		/// </summary>
		internal HttpPerformance(HttpContext context)
		{
			_context = context;
			BeginTime = DateTime.Now;
		}

		/// <summary>
		/// 性能计数已经更新
		/// </summary>
		public event EventHandler PerformanceUpdated;

		/// <summary>
		/// 引发 <see cref="PerformanceUpdated"/> 事件
		/// </summary>

		protected virtual void OnPerformanceUpdated()
		{
			PerformanceUpdated?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 设置启用速度统计
		/// </summary>
		public void EnableSpeedMonitor()
		{
			if (_speedTimer != null)
				return;

			_context.RequestDataSending += (s, e) =>
			{
				_inUpload = true;
				_dataProcessed = 0;
				_lastTicks = _startTicks = DateTime.Now.Ticks;
				_speedTimer.Start();
				OnPerformanceUpdated();
			};
			_context.RequestSended += (s, e) =>
			{
				_speedTimer.Stop();
				var ticks = DateTime.Now.Ticks - _startTicks;
				AverageUploadSpeed = ticks <= 0 ? (double?)null : _dataLength * 1.0 / ticks / (1000.0 * 10000.0);
				_dataLength = 0;
				_startTicks = 0L;
				UploadResetTime = null;
				InstantUploadSpeed = null;
				OnPerformanceUpdated();
			};
			_context.ResponseStreamFetched += (s, e) =>
			{
				_inUpload = false;
				_dataProcessed = 0;
				_lastTicks = _startTicks = DateTime.Now.Ticks;
				_speedTimer.Start();
				OnPerformanceUpdated();
			};
			_context.ResponseDataReceiveCompleted += (s, e) =>
			{
				_speedTimer.Stop();
				var ticks = DateTime.Now.Ticks - _startTicks;
				AverageDownloadSpeed = ticks <= 0 ? (double?)null : _dataLength * 1.0 / ticks / (1000.0 * 10000.0);
				_dataLength = 0;
				_startTicks = 0L;
				DownloadResetTime = null;
				InstantDownloadSpeed = null;
				OnPerformanceUpdated();
			};
			_context.RequestFinished += (s, e) =>
			{
				_speedTimer.Stop();
				_dataLength = 0;
				_startTicks = 0L;
				DownloadResetTime = null;
				InstantDownloadSpeed = null;
				OnPerformanceUpdated();
			};
			_context.RequestFailed += (s, e) =>
			{
				_speedTimer.Stop();
				_dataLength = 0;
				_startTicks = 0L;
				DownloadResetTime = null;
				InstantDownloadSpeed = null;
				OnPerformanceUpdated();
			};
			_context.RequestDataSendProgressChanged += (s, e) =>
			{
				_dataProcessed = e.BytesPassed;
				_dataLength = e.BytesCount;
			};
			_context.ResponseReadProgressChanged += (s, e) =>
			{
				_dataProcessed = e.BytesPassed;
				_dataLength = e.BytesCount;
			};
			_speedTimer = new Timer(_context.Client.Setting.SpeedMonitorInterval) { AutoReset = true };
			_speedTimer.Elapsed += (s, e) =>
			{
				var time = DateTime.Now.Ticks - _lastTicks;
				var dataLength = _dataProcessed - _dataProcessLast;
				var speed = time == 0 ? (double?)null : dataLength / (time / 1000.0 / 10000.0);

				var totalTime = DateTime.Now.Ticks - _startTicks;
				var speedAvg = totalTime == 0 ? (double?)null : _dataProcessed / (totalTime / 1000.0 / 10000.0);
				//大概剩余时间
				TimeSpan? resetTime = null;
				if (speed != null)
				{
					resetTime = new TimeSpan((long)((_dataLength - _dataProcessed) / speed.Value * 1000 * 10000));
				}

				if (_inUpload)
				{
					InstantUploadSpeed = speed;
					UploadResetTime = resetTime;
					AverageUploadSpeed = speedAvg;
				}
				else
				{
					InstantDownloadSpeed = speed;
					DownloadResetTime = resetTime;
					AverageDownloadSpeed = speedAvg;
				}
				_lastTicks = DateTime.Now.Ticks;
				_dataProcessLast = _dataProcessed;
				OnPerformanceUpdated();
			};
		}

		/// <summary>
		/// 当前下载速度（实时速度）
		/// </summary>
		public double? InstantDownloadSpeed { get; private set; }

		/// <summary>
		/// 平均下载速度
		/// </summary>
		public double? AverageDownloadSpeed { get; private set; }

		/// <summary>
		/// 平均上传速度
		/// </summary>
		public double? AverageUploadSpeed { get; private set; }

		/// <summary>
		/// 当前上传速度（实时速度）
		/// </summary>
		public double? InstantUploadSpeed { get; private set; }

		/// <summary>
		/// 开始请求的时间
		/// </summary>
		public DateTime? BeginTime { get; private set; }

		/// <summary>
		/// 请求初始化完成时间
		/// </summary>
		public DateTime? InitialzieCompleteTime { get; internal set; }

		/// <summary>
		/// 获得请求流的时间
		/// </summary>
		public DateTime? GetRequestStreamTime { get; internal set; }

		/// <summary>
		/// 完成写入请求流的时间
		/// </summary>
		public DateTime? CompleteRequestStreamTime { get; internal set; }

		/// <summary>
		/// 获得响应的时间
		/// </summary>
		public DateTime? GotResponseTime { get; internal set; }

		/// <summary>
		/// 获得响应流的时间
		/// </summary>
		public DateTime? GotResponseStreamTime { get; internal set; }


		/// <summary>
		/// 读取响应完成
		/// </summary>
		public DateTime? ReadResponseFinished { get; internal set; }

		/// <summary>
		/// 完成响应处理的时间
		/// </summary>
		public DateTime? FinishResponseTime { get; internal set; }

		/// <summary>
		/// 结束时间
		/// </summary>
		public DateTime? EndTime { get; internal set; }

		/// <summary>
		/// 获得请求消耗的时间
		/// </summary>
		public TimeSpan? ElapsedTime
		{
			get
			{
				if (BeginTime == null)
					return null;

				if (EndTime == null)
					return DateTime.Now - BeginTime.Value;

				return EndTime - BeginTime;
			}
		}

		/// <summary>
		/// 上传剩余时间
		/// </summary>
		public TimeSpan? UploadResetTime { get; private set; }

		/// <summary>
		/// 上传剩余时间
		/// </summary>
		public TimeSpan? DownloadResetTime { get; private set; }

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
			if (_disposed)
				return;
			_disposed = true;

			OnDisposed();

			if (disposing)
			{
				_speedTimer?.Dispose();

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

		/// <summary>
		/// 对象销毁时触发
		/// </summary>
		public event EventHandler Disposed;

		/// <summary>
		/// 引发 <see cref="Disposed" /> 事件
		/// </summary>
		protected virtual void OnDisposed()
		{
			var handler = Disposed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		#endregion


	}
}
