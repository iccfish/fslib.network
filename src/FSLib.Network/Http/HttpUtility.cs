using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Linq.Expressions;
	using System.Net;
	using System.Reflection;

	internal static class HttpUtility
	{
		static HttpUtility()
		{
		}

		/// <summary>
		/// 变更Uri中的主机URI
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="host"></param>
		/// <returns></returns>
		public static Uri ChangeHost(Uri uri, string host)
		{
			return new Uri(uri.Scheme + "://" + host + uri.PathAndQuery);
		}
	}
}
