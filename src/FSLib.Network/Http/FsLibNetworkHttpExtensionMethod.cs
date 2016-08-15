using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// HTTP相关扩展方法
	/// </summary>
	public static class FsLibNetworkHttpExtensionMethod
	{
		/// <summary>
		/// 判断当前请求是否成功
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public static bool IsSuccess(this HttpContext ctx)
		{
			return ctx != null && ctx.IsSuccess;
		}

		/// <summary>
		/// 获得错误信息
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public static string GetExceptionMessage(this HttpContext ctx, string defaultMessage = null)
		{
			return ctx.Exception.SelectValue(s => s.Message) ?? defaultMessage;
		}
	}
}
