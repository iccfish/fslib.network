using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Downloader
{
	/// <summary>
	/// 下载状态
	/// </summary>
	public enum DownloadState
	{
		/// <summary>
		/// 等待
		/// </summary>
		Pedding = 0,
		/// <summary>
		/// 连接服务器
		/// </summary>
		Connecting = 1,
		/// <summary>
		/// 正在下载
		/// </summary>
		Downloading = 2,
		/// <summary>
		/// 等待中
		/// </summary>
		Wait = 3,
		/// <summary>
		/// 成功
		/// </summary>
		Success = 4,
		/// <summary>
		/// 失败
		/// </summary>
		Error = 5
	}
}
