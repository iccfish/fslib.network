namespace FSLib.Network.Http.ObjectWrapper
{
	using System.Linq;

	/// <summary>
	/// 数据发送包装接口，表示当前对象支持将数据进行包装以便于在请求中发送
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRequestContentBuilder<T>
	{
		/// <summary>
		/// 创建数据类
		/// </summary>
		/// <param name="data">要发送的数据</param>
		/// <returns>用于发送指定数据的 <see cref="HttpRequestContent"/> 类</returns>
		HttpRequestContent BuildRequestContent(T data, RequestWrapRequestContentEventArgs ea);
	}
}
