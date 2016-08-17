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
			var resultType = typeof(Socket);

			try
			{
				var requestType = typeof(HttpWebRequest);
				var connectStreamType = requestType.Assembly.GetType("System.Net.ConnectStream");

				if (connectStreamType == null)
					return;

				var paramExp = Expression.Parameter(typeof(Stream), "stream");

#if NET_GT_4
				// .NET 4.5+
				var prop = connectStreamType.GetProperty("InternalSocket", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
				if (prop == null)
					return;

				var connectStreamExp = Expression.TypeAs(paramExp, connectStreamType);

				var propertyExp = Expression.Condition(
					Expression.ReferenceNotEqual(connectStreamExp, Expression.Constant(null, connectStreamType)),
					Expression.Property(connectStreamExp, prop),
					Expression.Constant(null, resultType)
				);

				_getRawSocketDelegate = Expression.Lambda<Func<Stream, Socket>>(propertyExp, paramExp).Compile();

#elif NET35
				// <= .NET 4.0
				var prop = connectStreamType.GetProperty("Connection", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
				if (prop == null)
					return;

				var connectionType = requestType.Assembly.GetType("System.Net.PooledStream");
				var getSocket = connectionType?.GetProperty("Socket", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
				if (getSocket == null)
					return;

				var getConnection = Expression.TypeAs(Expression.Property(paramExp, prop), connectionType);
				var propertyExp = Expression.Condition(
					Expression.Equal(getConnection, Expression.Constant(null, connectionType)),
					Expression.Property(getConnection, getSocket),
					Expression.Constant(null, resultType)
				);

				_getRawSocketDelegate = Expression.Lambda<Func<Stream, Socket>>(propertyExp, paramExp).Compile();
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
				if (fieldInfo == null)
					return;

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
