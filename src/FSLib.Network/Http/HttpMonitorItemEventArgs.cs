namespace FSLib.Network.Http
{
	using System;

	/// <summary>
	/// 记录事件数据
	/// </summary>
	public class HttpMonitorItemEventArgs : EventArgs
	{
		/// <summary>
		/// 获得事件相关联的类
		/// </summary>
		public HttpMonitorItem Item { get; private set; }

		/// <summary>
		/// 创建 <see cref="HttpMonitorItemEventArgs" />  的新实例(HttpMonitorItemEventArgs)
		/// </summary>
		/// <param name="item"></param>
		public HttpMonitorItemEventArgs(HttpMonitorItem item)
		{
			Item = item;
		}

	}
}