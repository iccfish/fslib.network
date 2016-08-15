using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 当前上下文会话信息
	/// </summary>
	public class ConnectionInfo
	{
		private Socket _rawSocket;

		internal ConnectionInfo()
		{

		}

		/// <summary>
		/// 刷新信息
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		internal void RefreshInfo(HttpWebRequest request, HttpWebResponse response)
		{
			var servicePoint = request.ServicePoint;
			Certificate = servicePoint.Certificate;
			ClientCertificate = servicePoint.ClientCertificate;
			ServicePointSupportPipelining = servicePoint.SupportsPipelining;
			ConnectionAddress = servicePoint.Address;
			ProtocolInfo = servicePoint.ProtocolVersion;

			ServicePoint = servicePoint;
		}

		internal void SetRequest(HttpWebRequest requestInfo, HttpWebResponse response)
		{
			if (ServerIPAddressList == null)
				ServerIPAddressList = SocketInfoProvider.GetRawSocketFromStream(requestInfo.ServicePoint);
		}

		/// <summary>
		/// 刷新访问数据
		/// </summary>
		/// <param name="isRequest"></param>
		/// <param name="stream"></param>
		internal void SetStream(bool isRequest, Stream stream)
		{
			if (RawSocket == null)
				RawSocket = SocketInfoProvider.GetRawSocketFromStream(stream);
		}

		/// <summary>
		/// 当前的服务器证书
		/// </summary>
		public X509Certificate Certificate { get; internal set; }

		/// <summary>
		/// 获得最后使用的客户端证书
		/// </summary>
		public X509Certificate ClientCertificate { get; private set; }

		/// <summary>
		/// 获得当前会话连接的远程节点
		/// </summary>
		public Uri ConnectionAddress { get; private set; }

		/// <summary>
		/// 获得请求的本地端口
		/// </summary>
		public IPEndPoint LocalIPEndPoint { get; private set; }

		/// <summary>
		/// 当前的协议版本
		/// </summary>
		public Version ProtocolInfo { get; private set; }

		/// <summary>
		/// 提供一个对原始Socket的只读访问。警告：请不要尝试对此Socket进行任何操作
		/// </summary>
		public Socket RawSocket
		{
			get { return _rawSocket; }
			private set
			{
				_rawSocket = value;
				LocalIPEndPoint = (IPEndPoint)_rawSocket?.LocalEndPoint;
				RemoteIPEndPoint = (IPEndPoint)_rawSocket?.RemoteEndPoint;
			}
		}

		/// <summary>
		/// 返回当前的Socket是否还连接
		/// </summary>
		public bool? IsSocketConnected => RawSocket?.Connected;

		/// <summary>
		/// 获得已解析的服务器IP列表
		/// </summary>
		public IPAddress[] ServerIPAddressList { get; private set; }

		/// <summary>
		/// 远程节点
		/// </summary>
		public IPEndPoint RemoteIPEndPoint { get; private set; }

		/// <summary>
		/// 获得相关联的服务器连接
		/// </summary>
		public ServicePoint ServicePoint { get; private set; }

		/// <summary>
		/// 获得相应最后的服务节点是否支持Pipelining
		/// </summary>
		public bool ServicePointSupportPipelining { get; private set; }

	}
}
