using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 请求状态
	/// </summary>
	public enum HttpContextState
	{
		/// <summary>
		/// 未发送
		/// </summary>
		NotSended = 0,
		/// <summary>
		/// 正在初始化
		/// </summary>
		Init = 100,
		/// <summary>
		/// 正在发送标头
		/// </summary>
		SendingRequestHeader = 200,
		/// <summary>
		/// 正在写入请求数据
		/// </summary>
		WriteRequestData = 300,
		/// <summary>
		/// 等待请求标头
		/// </summary>
		WaitingResponseHeader = 400,
		/// <summary>
		/// 正在读取响应
		/// </summary>
		ReadingResponse = 600,
		/// <summary>
		/// 完成处理响应
		/// </summary>
		EndProcessResponse = 700,
		/// <summary>
		/// 正在验证响应
		/// </summary>
		ValidatingResponse = 800,
		/// <summary>
		/// 完成请求
		/// </summary>
		Complete = 900
	}
}
