using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LibEveryFileExplorer.Files.SimpleFileSystem
{
	public class SFSDirectory
	{
		public SFSDirectory(string Name, bool Root)
		{
			DirectoryName = Name;
			IsRoot = Root;
			SubDirectories = new List<SFSDirectory>();
			Files = new List<SFSFile>();
		}
		public SFSDirectory(UInt16 Id)
		{
			DirectoryID = Id;
			IsRoot = false;
			SubDirectories = new List<SFSDirectory>();
			Files = new List<SFSFile>();
		}
		public String DirectoryName;
		public UInt16 DirectoryID;

		public SFSDirectory Parent;

		public List<SFSDirectory> SubDirectories;
		public List<SFSFile> Files;

		public Boolean IsRoot { get; set; }

		public SFSFile this[string index]
		{
			get
			{
				foreach (SFSFile f in Files)
				{
					if (f.FileName == index) return f;
				}
				return null;
				//throw new FileNotFoundException("The file " + index + " does not exist in this directory.");
			}
			set
			{
				for (int i = 0; i < Files.Count; i++)
				{
					if (Files[i].FileName == index)
					{
						Files[i] = value;
						return;
					}
				}
				throw new System.IO.FileNotFoundException("The file " + index + " does not exist in this directory.");
			}
		}

		public UInt32 TotalNrSubDirectories
		{
			get
			{
				UInt32 nr = (UInt32)SubDirectories.Count;
				foreach (SFSDirectory d in SubDirectories) nr += d.TotalNrSubDirectories;
				return nr;
			}
		}

		public UInt32 TotalNrSubFiles
		{
			get
			{
				UInt32 nr = (UInt32)Files.Count;
				foreach (SFSDirectory d in SubDirectories) nr += d.TotalNrSubFiles;
				return nr;
			}
		}

		public SFSFile GetFileByID(int Id)
		{
			foreach (SFSFile f in Files)
			{
				if (f.FileID == Id) return f;
			}
			foreach (SFSDirectory d in SubDirectories)
			{
				SFSFile f = d.GetFileByID(Id);
				if (f != null) return f;
			}
			return null;
		}

		public TreeNode GetTreeNodes()
		{
			TreeNode Root = new TreeNode(DirectoryName);
			Root.ImageIndex = 0;
			foreach (var v in SubDirectories)
			{
				Root.Nodes.Add(v.GetTreeNodes());
			}
			return Root;
		}

		public ListViewItem[] GetContent()
		{
			List<ListViewItem> Items = new List<ListViewItem>();
			foreach (var v in SubDirectories)
			{
				Items.Add(new ListViewItem(v.DirectoryName, "Folder") { Tag = "Folders" });
			}
			foreach (var v in Files)
			{
				Items.Add(EveryFileExplorerUtil.GetFileItem(new EFESFSFile(v)));
			}
			return Items.ToArray();
		}

		public SFSDirectory GetDirectoryByPath(String Path)
		{
			if (Path.TrimStart('/').StartsWith("../"))
			{
				if (Parent == null) return null;
				return Parent.GetDirectoryByPath("/" + Path.TrimStart('/').Substring(2));
			}
			if (!Path.StartsWith(DirectoryName)) return null;
			if (Path == DirectoryName) return this;
			Path = Path.Substring(DirectoryName.Length);
			if (Path.Length == 0 || !Path.StartsWith("/")) return null;
			String dir = Path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
			foreach(var v in SubDirectories)
			{
				if(v.DirectoryName == dir) return v.GetDirectoryByPath(Path.Substring(1));
			}
			return null;
		}

		public SFSFile GetFileByPath(String Path)
		{
			if (Path.TrimStart('/').StartsWith("../"))
			{
				if (Parent == null) return null;
				return Parent.GetFileByPath("/" + Path.TrimStart('/').Substring(2));
			}
			if (!Path.StartsWith(DirectoryName)) return null;
			Path = Path.Substring(DirectoryName.Length);
			if (Path.Length == 0 || !Path.StartsWith("/")) return null;
			//Sarc files may use slashes in their names (since they are just hashes, and do not have a real directory structure)
			var vvv = this[Path.Substring(1)];
			if (vvv != null) return vvv;

			string[] parts = Path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > 1)
			{
				String dir = parts[0];
				foreach (var v in SubDirectories)
				{
					if (v.DirectoryName == dir) return v.GetFileByPath(Path.Substring(1));
				}
			}
			else if(parts.Length == 1)
			{
				return this[parts[0]];
			}
			return null;
		}

		public void UpdateIDs(ref int DirId, ref int FileID)
		{
			this.DirectoryID = (ushort)(0xF000 | DirId++);
			foreach (var v in Files)
			{
				v.FileID = FileID++;
			}
			foreach (var v in SubDirectories)
			{
				v.UpdateIDs(ref DirId, ref FileID);
			}
		}

		public bool IsValidName(String Name)
		{
			char[] invalid = System.IO.Path.GetInvalidFileNameChars();
			foreach (char c in invalid)
			{
				if (Name.Contains(c)) return false;
			}
			foreach (var v in SubDirectories)
			{
				if (v.DirectoryName == Name) return false;
			}
			foreach (var v in Files)
			{
				if (v.FileName == Name) return false;
			}
			return true;
		}

		public void PrintTree(ref int indent)
		{
			Console.WriteLine(new string(' ', indent * 4) + DirectoryName + " (" + DirectoryID.ToString("X") + ")");
			indent++;
			foreach (var v in Files)
			{
				Console.WriteLine(new string(' ', indent * 4) + v.FileName + " (" + v.FileID.ToString("X") + ")");
			}
			foreach (var v in SubDirectories)
			{
				v.PrintTree(ref indent);
			}
			indent--;
		}

		public void Export(String Path)
		{
			foreach (var v in Files)
			{
				System.IO.File.Create(Path + "\\" + v.FileName).Close();
				System.IO.File.WriteAllBytes(Path + "\\" + v.FileName, v.Data);
			}
			foreach (var v in SubDirectories)
			{
				System.IO.Directory.CreateDirectory(Path + "\\" + v.DirectoryName);
				v.Export(Path + "\\" + v.DirectoryName);
			}
		}

		public override string ToString()
		{
			if (Parent != this)
			{
				if (!IsRoot) return Parent.ToString() + DirectoryName + "/";
				else return "/";
			}
			else return DirectoryName;
		}
	}
}
