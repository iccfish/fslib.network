namespace FSLib.Network.Http
{
	/// <summary>
	/// 返回的结果类型
	/// </summary>
	public enum ResponseContentType
	{
		/// <summary>
		/// 未知类型
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// JSON格式
		/// </summary>
		Json = 1,
		/// <summary>
		/// JSONP格式
		/// </summary>
		JsonP = 2,
		/// <summary>
		/// XML格式
		/// </summary>
		Xml = 3,
		/// <summary>
		/// 二进制格式
		/// </summary>
		Binary = 4
	}
}