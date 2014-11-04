using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files.SimpleFileSystem;

namespace _3DS
{
	public class SARCEFESFSFile : EFESFSFile
	{
		public SARCEFESFSFile(SFSFile File)
			: base(File) { }

		public override byte[] FindFileRelative(string Path)
		{
			Path = Path.Replace("\\", "/");
			if (Path.TrimStart('/').StartsWith("../")) return null;
			return File.Parent.GetFileByID((int)SARC.GetHashFromName(Path, 0x65)).Data;
		}
	}
}
