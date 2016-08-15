using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	///  表示当前类对象的发送方式
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ContentTypeAttribute : Attribute
	{
		/// <summary>
		/// 获得ContentType值
		/// </summary>
		public ContentType ContentType { get; private set; }

		/// <summary>
		/// 创建 <see cref="ContentTypeAttribute" />  的新实例(RequestPayloadAttribute)
		/// </summary>
		/// <param name="contentType"></param>
		public ContentTypeAttribute(ContentType contentType)
		{
			ContentType = contentType;
		}
	}
}
