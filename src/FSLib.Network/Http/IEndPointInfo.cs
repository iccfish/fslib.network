namespace FSLib.Network.Http
{
	using System.Net;

	/// <summary>
	///与请求相关的终端信息
	/// </summary>
	public interface IEndPointInfo
	{
		/// <summary>
		/// 获得服务器的终端信息
		/// </summary>
		IPEndPoint RemoteEndPoint { get; }

		/// <summary>
		/// 获得本地终端信息
		/// </summary>
		IPEndPoint LocalEndPoint { get; }

		/// <summary>
		/// 相关联的IP地址信息
		/// </summary>
		ServicePoint ServicePoint { get; }
	}
}