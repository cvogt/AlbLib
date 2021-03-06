﻿using System;
using System.IO;

namespace AlbLib
{
	/// <summary>
	/// Writes data to stream.
	/// </summary>
	public interface IGameResource
	{
		/// <summary>
		/// Writes data to <paramref name="output"/> stream.
		/// </summary>
		/// <param name="output">
		/// Output stream.
		/// </param>
		/// <returns>
		/// Number of written bytes.
		/// </returns>
		int Save(Stream output);
	}
}
