using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 对象不支持序列化异常
	/// </summary>
	public class ObjectSerializationNotSupportException : Exception
	{
		/// <summary>
		/// 新建 <see cref="ObjectSerializationNotSupportException"/> 对象
		/// </summary>
		/// <param name="type"></param>
		/// <param name="ex"></param>
		public ObjectSerializationNotSupportException(Type type, Exception ex) : base(SR.httpexection_objectserializationnotsupport.FormatWith(type.AssemblyQualifiedName), ex)
		{

		}
	}
}
