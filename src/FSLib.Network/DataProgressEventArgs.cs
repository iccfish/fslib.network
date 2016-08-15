using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network
{
	/// <summary>
	/// 数据事件
	/// </summary>
	public class DataProgressEventArgs : EventArgs
	{
		/// <summary>
		/// 获得长度
		/// </summary>
		public long BytesCount { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public long BytesPassed { get; private set; }

		/// <summary>
		/// 获得当前操作的进度
		/// </summary>
		public double Percentage
		{
			get
			{
				if (BytesCount < 1) return -1;
				return BytesPassed * 1.0 / BytesCount;
			}
		}

		/// <summary>
		/// 创建 <see cref="DataProgressEventArgs" />  的新实例(DataProgressEventArgs)
		/// </summary>
		public DataProgressEventArgs(long bytesCount, long bytesPassed)
		{
			BytesCount = bytesCount;
			BytesPassed = bytesPassed;
		}
	}
}
