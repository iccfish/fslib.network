using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Downloader
{
	using System.Collections;

	/// <summary>
	/// 下载分片集合
	/// </summary>
	public class MultiThreadPieceCollection : IEnumerable<MultiThreadDownloadPiece>
	{
		List<MultiThreadDownloadPiece> _pieces = new List<MultiThreadDownloadPiece>();

		/// <summary>
		/// 返回一个循环访问集合的枚举器。
		/// </summary>
		/// <returns>
		/// 可用于循环访问集合的 <see cref="T:System.Collections.Generic.IEnumerator`1"/>。
		/// </returns>
		public IEnumerator<MultiThreadDownloadPiece> GetEnumerator()
		{
			foreach (var piece in _pieces.ToArray())
			{
				yield return piece;
			}
		}

		/// <summary>
		/// 返回一个循环访问集合的枚举数。
		/// </summary>
		/// <returns>
		/// 一个可用于循环访问集合的 <see cref="T:System.Collections.IEnumerator"/> 对象。
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
