using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// HTTP请求方法
	/// </summary>
	public enum HttpMethod
	{
		/// <summary>
		/// GET
		/// </summary>
		Get,
		/// <summary>
		/// POST
		/// </summary>
		Post,
		/// <summary>
		/// HEAD
		/// </summary>
		Head,
		/// <summary>
		/// OPTION
		/// </summary>
		Option,
		Patch,
		Delete,
		Link,
		Unlink,
		Purge,
		Options,
		Put,
		Connect,
		Copy,
		PropFind,
		Lock,
		Unlock
	}
}
