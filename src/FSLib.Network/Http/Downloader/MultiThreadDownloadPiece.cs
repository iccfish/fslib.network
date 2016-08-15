using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Downloader
{
	using System.IO;

	/// <summary>
	/// 多线程下载中的块
	/// </summary>
	public class MultiThreadDownloadPiece : DownloadInfo
	{
		/// <summary>
		/// 结束字节数
		/// </summary>
		public virtual long From { get; set; }

		/// <summary>
		/// 开始字节数
		/// </summary>
		public virtual long? To { get; set; }

		/// <summary>
		/// 当前负责下载的长度
		/// </summary>
		public virtual long DownloadLength { get; set; }

		/// <summary>
		/// 获得当前的下载状态信息
		/// </summary>
		public DownloadInfo DownloadInfo { get; set; }

	}
}
