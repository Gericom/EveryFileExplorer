using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;

namespace EveryFileExplorer.Files
{
	public class EFEDiskFile : EFEFile
	{
		public EFEDiskFile()
		{
			Name = "Untitled";
			Data = new byte[0];
		}

		public EFEDiskFile(String Path)
		{
			if (!File.Exists(Path)) throw new ArgumentException("File doesn't exist!");
			this.Path = Path;
			Name = System.IO.Path.GetFileName(Path);
			PrepareDataForUse(File.ReadAllBytes(Path));
		}

		public String Path { get; set; }

		public override void Save()
		{
			byte[] Data = GetDataForSave();
			File.Create(Path).Close();
			File.WriteAllBytes(Path, Data);
		}

		public override byte[] FindFileRelative(string Path)
		{
			Path = Path.Replace("/", "\\");
			String NewPath = System.IO.Path.GetDirectoryName(this.Path) + "\\" + Path;
			if (File.Exists(NewPath)) return File.ReadAllBytes(NewPath);
			return null;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is EFEDiskFile)) return false;
			EFEDiskFile a = (EFEDiskFile)obj;
			if (a.Path == Path) return true;
			return false;
		}
	}
}
