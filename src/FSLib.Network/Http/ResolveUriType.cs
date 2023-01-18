using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 解析的地址类型
	/// </summary>
	public enum ResolveUriType
	{
		/// <summary>
		/// 解析后的地址用于请求
		/// </summary>
		RequestUri,

		/// <summary>
		/// 解析后的地址将用于引用页
		/// </summary>
		ReferUri
	}
}
