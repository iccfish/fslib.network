using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 表示对象序列化属性
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ObjectSerializeAttribute : Attribute
	{
		/// <summary>
		/// 获得序列化类型
		/// </summary>
		public ObjectSerializationType SerializeType { get; private set; }

		/// <summary>
		/// 创建 <see cref="ObjectSerializeAttribute" />  的新实例(ObjectSerializeAttribute)
		/// </summary>
		public ObjectSerializeAttribute(ObjectSerializationType serializeType)
		{
			SerializeType = serializeType;
		}
	}
}
