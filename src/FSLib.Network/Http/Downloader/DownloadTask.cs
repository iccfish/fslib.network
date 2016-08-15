using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Downloader
{
	using System.IO;
	using System.Net;

	/// <summary>
	/// 下载任务
	/// </summary>
	public class DownloadTask
	{
		/// <summary>
		/// 源URI
		/// </summary>
		public string Uri { get; set; }

		/// <summary>
		/// 请求方法
		/// </summary>
		public HttpMethod HttpMethod { get; set; }

		/// <summary>
		/// 请求对象
		/// </summary>
		public object RequestObject { get; set; }

		/// <summary>
		/// Cookies
		/// </summary>
		public string Cookies { get; set; }

		/// <summary>
		/// 获得或设置使用的代理
		/// </summary>
		public IWebProxy WebProxy { get; set; }

		/// <summary>
		/// 目标文件
		/// </summary>
		public Stream TargetStream { get; set; }

		/// <summary>
		/// 是否预先分配空间
		/// </summary>
		public bool PreAllocSpace { get; set; }

		/// <summary>
		/// 缓冲区大小
		/// </summary>
		public int BufferSize { get; set; }

		/// <summary>
		/// 获得当前的下载状态信息
		/// </summary>
		public DownloadInfo DownloadInfo { get; set; }
	}
}
