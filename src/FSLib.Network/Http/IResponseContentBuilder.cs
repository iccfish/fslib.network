using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 数据接收处理接口
	/// </summary>
	/// <typeparam name="T">要接收的数据类型</typeparam>
	public interface IResponseContentBuilder<T>
	{
		/// <summary>
		/// 创建数据类
		/// </summary>
		/// <param name="data">要发送的数据</param>
		/// <returns>用于发送指定数据的 <see cref="HttpRequestContent"/> 类</returns>
		HttpResponseContent BuildResponseContentWrap(T data, GetPreferedResponseTypeEventArgs<T> ea);
	}
}
