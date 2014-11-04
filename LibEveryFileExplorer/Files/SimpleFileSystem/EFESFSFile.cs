using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Files.SimpleFileSystem
{
	public class EFESFSFile : EFEFile
	{
		public EFESFSFile(SFSFile File)
		{
			this.File = File;
			this.Parent = Parent;
			if (Parent != null) Parent.Children.Add(this);
			Name = File.FileName;
			PrepareDataForUse(File.Data);
		}

		public SFSFile File { get; private set; }

		public override void Save()
		{
			File.Data = GetDataForSave();
			if (Parent != null)
			{
				ViewableFile v = EveryFileExplorerUtil.GetViewableFileFromFile(Parent);
				if (v.Dialog is IChildReactive) ((IChildReactive)v.Dialog).OnChildSave(EveryFileExplorerUtil.GetViewableFileFromFile(this));
			}
		}

		public override byte[] FindFileRelative(string Path)
		{
			Path = Path.Replace("\\", "/");
			var v = File.Parent.GetFileByPath(Path);
			if (v != null) return File.Parent.GetFileByPath(Path).Data;
			return null;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is EFESFSFile)) return false;
			EFESFSFile a = (EFESFSFile)obj;
			if (a.File == File) return true;
			return false;
		}
	}
}
