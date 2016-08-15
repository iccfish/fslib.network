namespace FSLib.Network.Http
{
	using System;
	using System.Net;

	/// <summary>
	/// 地址族不匹配错误
	/// </summary>
	public class IPAddressFamilyMismatchException : Exception
	{
		/// <summary>
		/// 远程地址
		/// </summary>
		public IPEndPoint RemoteEndPoint { get; private set; }

		/// <summary>
		/// 本地地址
		/// </summary>
		public IPEndPoint LocalEndpoint { get; private set; }

		/// <summary>
		/// 新建对象
		/// </summary>
		/// <param name="remoteEndPoint"></param>
		/// <param name="localEndpoint"></param>
		public IPAddressFamilyMismatchException(IPEndPoint remoteEndPoint, IPEndPoint localEndpoint)
			: base($"无效的IP绑定。远程地址为 {remoteEndPoint}({remoteEndPoint.AddressFamily})，而绑定的本地地址类型为 {localEndpoint}({localEndpoint.AddressFamily})")
		{
			RemoteEndPoint = remoteEndPoint;
			LocalEndpoint = localEndpoint;
		}
	}
}