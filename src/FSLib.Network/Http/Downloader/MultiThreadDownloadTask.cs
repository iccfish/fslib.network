using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Downloader
{
	/// <summary>
	/// 多线程下载任务
	/// </summary>
	public class MultiThreadDownloadTask : DownloadTask
	{
		/// <summary>
		/// 线程数
		/// </summary>
		public int ThreadCount { get; set; }

		/// <summary>
		/// 最小允许的块尺寸
		/// </summary>
		public int MiniumPieceSize { get; set; } = 1 * 1024 * 1024;

		/// <summary>
		/// 下载分块
		/// </summary>
		public MultiThreadPieceCollection PieceCollection { get; private set; } = new MultiThreadPieceCollection();

		
	}
}
