using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Compression
{
	public interface CompressionFormatBase
	{
		byte[] Decompress(byte[] Data);
	}

	public abstract class CompressionFormat<T> : CompressionFormatBase where T : CompressionFormatIdentifier, new()
	{
		private static T _identifier = new T();
		public static T Identifier { get { return _identifier; } }

		public abstract byte[] Decompress(byte[] Data);
	}

	public abstract class CompressionFormatIdentifier
	{
		public abstract String GetCompressionDescription();
		public abstract bool IsFormat(byte[] Data);
	}
}
