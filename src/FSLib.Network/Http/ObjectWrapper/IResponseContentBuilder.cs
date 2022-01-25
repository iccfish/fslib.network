namespace FSLib.Network.Http.ObjectWrapper
{
	using System.Linq;

	/// <summary>
	/// 数据接收处理接口
	/// </summary>
	public interface IResponseContentBuilder
	{
		/// <summary>
		/// 创建数据类
		/// </summary>
		/// <returns>用于发送指定数据的 <see cref="HttpRequestContent"/> 类</returns>
		HttpResponseContent BuildResponseContentWrap(GetPreferredResponseTypeEventArgs ea);
	}
}
