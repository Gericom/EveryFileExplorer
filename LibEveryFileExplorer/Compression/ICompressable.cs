using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Compression
{
	public interface ICompressable
	{
		byte[] Compress(byte[] Data);
	}
}
