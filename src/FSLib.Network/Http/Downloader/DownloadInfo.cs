using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Downloader
{
	using System.IO;

	/// <summary>
	/// 下载状态信息
	/// </summary>
	public class DownloadInfo
	{
		/// <summary>
		/// 总长度
		/// </summary>
		public virtual long TotalLength { get; set; }


		/// <summary>
		/// 当前下载的位置
		/// </summary>
		public virtual long DownloadPosition { get; set; }

		/// <summary>
		/// 已下载长度
		/// </summary>
		public virtual long DownloadedLength { get; set; }

		/// <summary>
		/// 缓冲流
		/// </summary>
		public virtual MemoryStream BufferStream { get; set; }
	}
}
