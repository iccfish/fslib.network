namespace FSLib.Network.Http.ObjectWrapper;

/// <summary>
/// 用于封装载荷的工厂类型
/// </summary>
public interface IContentPayloadFactory
{
	/// <summary>
	/// 包装内容
	/// </summary>
	/// <param name="ea">请求参数</param>
	/// <returns></returns>
	HttpRequestContent WrapRequestContent(RequestWrapRequestContentEventArgs ea);

	/// <summary>
	/// 包装内容
	/// </summary>
	/// <param name="ea">请求参数</param>
	/// <returns></returns>
	void GetResponseContent(GetPreferredResponseTypeEventArgs ea);
}
