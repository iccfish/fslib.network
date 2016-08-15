using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// Cookies处理逻辑
	/// </summary>
	public enum CookiesHandleMethod
	{
		/// <summary>
		/// 默认处理逻辑，自动跟踪处理
		/// </summary>
		Auto = 0,
		/// <summary>
		/// 忽略Cookies
		/// </summary>
		Ignore = 1,
		/// <summary>
		/// 仅发送，不接收
		/// </summary>
		OnlySendWithoutReceive = 2
	}
}
