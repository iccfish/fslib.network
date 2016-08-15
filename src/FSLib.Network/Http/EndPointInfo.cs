using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Net;

	/// <summary>
	/// 与请求相关的终端信息
	/// </summary>
	public class EndPointInfo : IEndPointInfo
	{
		/// <summary>
		/// 获得服务器的终端信息
		/// </summary>
		public IPEndPoint RemoteEndPoint { get; set; }

		/// <summary>
		/// 获得本地终端信息
		/// </summary>
		public IPEndPoint LocalEndPoint { get; set; }

		/// <summary>
		/// 相关联的IP地址信息
		/// </summary>
		public ServicePoint ServicePoint { get; set; }
	}
}
