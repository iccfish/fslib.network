namespace FSLib.Network.Http.ObjectWrapper
{
	using System;

	/// <summary>
	/// 标记当前类型使用的数据处理类
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ResponseContentBuilderAttribute : Attribute
	{
		/// <summary>
		/// 数据处理类类型
		/// </summary>
		public Type Type { get; private set; }


		/// <summary>
		/// 创建 <see cref="ResponseContentBuilderAttribute" />  的新实例(ResponseContentBuilderAttribute)
		/// </summary>
		/// <param name="type"></param>
		public ResponseContentBuilderAttribute(Type type)
		{
			Type = type;
		}
	}
}