using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Collections;
	using System.Threading;

	/// <summary>
	/// HTTP请求监控类
	/// </summary>
	public class HttpMonitor
	{
		Queue<HttpMonitorItem> _items = new Queue<HttpMonitorItem>();
		object _lockObject = new object();
		SynchronizationContext _context;

		/// <summary>
		/// 记录被清空
		/// </summary>
		public event EventHandler Cleared;

		/// <summary>
		/// 引发 <see cref="Cleared" /> 事件
		/// </summary>
		protected virtual void OnCleared()
		{
			var handler = Cleared;
			if (handler == null)
				return;
			if (_context == null)
			{
				handler(this, EventArgs.Empty);
			}
			else
			{
				_context.Post(_ => handler(this, EventArgs.Empty), null);
			}
		}

		/// <summary>
		/// 有新纪录
		/// </summary>
		public event EventHandler<HttpMonitorItemEventArgs> ItemWatched;

		/// <summary>
		/// 引发 <see cref="ItemWatched" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnItemWatched(HttpMonitorItemEventArgs ea)
		{
			var handler = ItemWatched;

			if (handler == null)
				return;
			if (_context == null)
			{
				handler(this, ea);
			}
			else
			{
				_context.Post(_ => handler(this, ea), null);
			}
		}

		/// <summary>
		/// 记录被移除
		/// </summary>
		public event EventHandler<HttpMonitorItemEventArgs> ItemRemoved;


		/// <summary>
		/// 引发 <see cref="ItemRemoved" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnItemRemoved(HttpMonitorItemEventArgs ea)
		{
			var handler = ItemRemoved;

			if (handler == null)
				return;
			if (_context == null)
			{
				handler(this, ea);
			}
			else
			{
				_context.Post(_ => handler(this, ea), null);
			}
		}

		/// <summary>
		/// 获得或设置最多记录的条数
		/// </summary>
		public int MaxRecordItems { get; set; }

		/// <summary>
		/// 获得或设置是否记录请求内容
		/// </summary>
		public bool RecordRequestContent { get; set; }

		/// <summary>
		/// 获得或记录是否记录响应内容
		/// </summary>
		public bool RecordResponseContent { get; set; }

		/// <summary>
		/// 获得或设置是否记录原始的响应内容
		/// </summary>
		public bool RecordRawResponseContent { get; set; }

		/// <summary>
		/// 获得或设置记录的最大内容长度。此值设置过高将会导致内存占用过高。
		/// </summary>
		public long MaxRecordContentSize { get; set; }

		/// <summary>
		/// 创建 <see cref="HttpMonitor" />  的新实例(Monitor)
		/// </summary>
		public HttpMonitor()
		{
			_context = SynchronizationContext.Current;
		}

		/// <summary>
		/// 向监控类中注册源
		/// </summary>
		/// <param name="contxt"></param>
		internal HttpMonitorItem Register(HttpContext contxt)
		{
			lock (_lockObject)
			{
				var source = new HttpMonitorItem(contxt, this);
				_items.Enqueue(source);
				OnItemWatched(new HttpMonitorItemEventArgs(source));

				if (MaxRecordItems > 0)
				{
					while (_items.Count > MaxRecordItems)
					{
						_items.Dequeue();
					}
				}

				return source;
			}
		}

		/// <summary>
		/// 获取集合中的元素数。
		/// </summary>
		/// <returns>
		/// 集合中的元素数。
		/// </returns>
		public int Count
		{
			get { return _items.Count; }
		}

		/// <summary>
		/// 获得所有记录
		/// </summary>
		/// <returns></returns>
		public IEnumerable<HttpMonitorItem> GetAllItems()
		{
			lock (_lockObject)
			{
				return _items.ToArray();
			}
		}

		/// <summary>
		/// 清除所有记录
		/// </summary>
		public void Clear()
		{
			_items.Clear();
		}
	}
}
