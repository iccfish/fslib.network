namespace FSLib.Network.Http
{
	using System;

	/// <summary>
	/// 标记当前类型使用的数据处理类
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class RequestContentBuilderAttribute : Attribute
	{
		/// <summary>
		/// 数据处理类类型
		/// </summary>
		public Type Type { get; private set; }


		/// <summary>
		/// 创建 <see cref="ResponseContentBuilderAttribute" />  的新实例(ResponseContentBuilderAttribute)
		/// </summary>
		/// <param name="type"></param>
		public RequestContentBuilderAttribute(Type type)
		{
			Type = type;
		}
	}
}