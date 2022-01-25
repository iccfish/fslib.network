namespace FSLib.Network.Http.ObjectWrapper
{
	/// <summary>
	/// 对Response和Request中数据的默认承载
	/// </summary>
	public interface IContentPayloadBuilder
	{
		/// <summary>
		/// 获得响应内容的默认承载
		/// </summary>
		/// <returns>负责接收并解析数据的 <see cref="HttpResponseContent"/></returns>
		/// <returns></returns>
		void GetResponseContent(GetPreferredResponseTypeEventArgs ea);

		/// <summary>
		/// 对请求数据进行封装
		/// </summary>
		/// <returns>包装了数据的 <see cref="HttpRequestContent"/></returns>
		HttpRequestContent WrapRequestContent(RequestWrapRequestContentEventArgs ea);
	}
}