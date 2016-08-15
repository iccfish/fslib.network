namespace FSLib.Network.Http
{
	/// <summary>
	/// 对Response和Request中数据的默认承载
	/// </summary>
	public interface IContentPayloadBuilder
	{
		/// <summary>
		/// 获得响应内容的默认承载
		/// </summary>
		/// <param name="data">期望的数据类型的默认实例（如果给出已有数据，则为null）</param>
		/// <returns>负责接收并解析数据的 <see cref="HttpResponseContent"/></returns>
		/// <returns></returns>
		HttpResponseContent GetResponseContent<T>(T data, GetPreferedResponseTypeEventArgs<T> ea);

		/// <summary>
		/// 对请求数据进行封装
		/// </summary>
		/// <param name="data">请求数据</param>
		/// <returns>包装了数据的 <see cref="HttpRequestContent"/></returns>
		HttpRequestContent WrapRequestContent(object data, RequestWrapRequestContentEventArgs ea);
	}
}