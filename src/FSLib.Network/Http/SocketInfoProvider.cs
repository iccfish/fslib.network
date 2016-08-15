using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace FSLib.Network.Http
{
	class SocketInfoProvider
	{
		private static Func<Stream, Socket> _getRawSocketDelegate;
		private static Func<ServicePoint, IPAddress[]> _getIPAddressListDelegate;

		static SocketInfoProvider()
		{
			BuildGetSocketDelegate();
			BuildGetAddressListDelegate();
		}

		#region 创建委托

		static void BuildGetSocketDelegate()
		{
			try
			{
				var requestType = typeof(HttpWebRequest);
				var connectStreamType = requestType.Assembly.GetType("System.Net.ConnectStream");
				var paramExp = Expression.Parameter(typeof(Stream), "stream");

#if NET_GT_4
				// .NET 4.5+
				var prop = connectStreamType.GetProperty("InternalSocket", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
				var connectStreamExp = Expression.Convert(paramExp, connectStreamType);
				var propertyExp = Expression.Property(connectStreamExp, prop);

				_getRawSocketDelegate = Expression.Lambda<Func<Stream, Socket>>(propertyExp, paramExp).Compile();

#else
				// <= .NET 4.0
				var prop = connectStreamType.GetProperty("Connection", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
				var connectionType = requestType.Assembly.GetType("System.Net.PooledStream");
				var getConnection = Expression.Convert(Expression.Property(paramExp, prop), connectionType);
				var getSocket = connectionType.GetProperty("Socket", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);

				_getRawSocketDelegate = Expression.Lambda<Func<Stream, Socket>>(Expression.Property(getConnection, getSocket), paramExp).Compile();
#endif
			}
			catch (Exception e)
			{
				Trace.TraceWarning($"Running [FSLib.Network.Http.BuildGetSocketDelegate] Failed due to {e}");
			}
		}

		static void BuildGetAddressListDelegate()
		{
			try
			{
				var sptype = typeof(ServicePoint);
				var fieldInfo = sptype.GetField("m_IPAddressInfoList", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
				var exp1param = Expression.Parameter(sptype, "servicePoint");
				var expaccess = Expression.Field(exp1param, fieldInfo);
				_getIPAddressListDelegate = Expression.Lambda<Func<ServicePoint, IPAddress[]>>(expaccess, exp1param).Compile();
			}
			catch (Exception e)
			{
				Trace.TraceWarning($"Running [FSLib.Network.Http.BuildGetAddressListDelegate] Failed due to {e}");
			}
		}

		#endregion

		/// <summary>
		/// 从连接流里获得原始Socket
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Socket GetRawSocketFromStream(Stream stream) => _getRawSocketDelegate?.Invoke(stream);

		/// <summary>
		/// 从ServicePoint里获得服务器地址列表
		/// </summary>
		/// <param name="servicePoint"></param>
		/// <returns></returns>
		public static IPAddress[] GetRawSocketFromStream(ServicePoint servicePoint) => _getIPAddressListDelegate?.Invoke(servicePoint);
	}
}
