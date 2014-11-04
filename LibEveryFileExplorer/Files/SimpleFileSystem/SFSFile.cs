using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Files.SimpleFileSystem
{
	public class SFSFile
	{
		public SFSFile(Int32 Id, String Name, SFSDirectory Parent)
		{
			FileID = Id;
			FileName = Name;
			this.Parent = Parent;
		}
		public String FileName;
		public Int32 FileID;
		public Byte[] Data;

		public SFSDirectory Parent;

		public override string ToString()
		{
			return Parent.ToString() + FileName;
		}
	}
}
