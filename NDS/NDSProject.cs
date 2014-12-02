using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Projects;
using System.Windows.Forms;
using System.Xml.Serialization;
using NDS.UI;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;

namespace NDS
{
	public class NDSProject : Project<NDSProject.NDSProjectIdentifier, NDSProject.NDSProjectFile>
	{
		public NDSProject() { }

		public NDSProject(String ProjectFile)
			: base(ProjectFile) { }

		public override bool CreateNew()
		{
			var v = new NDSProjectDialog();
			if (v.ShowDialog() == DialogResult.OK)
			{
				ProjectFile = new NDSProjectFile();
				ProjectFile.ProjectName = v.ProjectName;
				ProjectDir = v.ProjectDir;
				var NDSFile = new Nitro.NDS(File.ReadAllBytes(v.NDSPath));
				var FS = NDSFile.ToFileSystem();
				var Dir = new DirectoryInfo(ProjectDir);
				FS.Export(Dir.CreateSubdirectory("data").FullName);
				Dir.CreateSubdirectory("overlay");
				foreach (var vv in NDSFile.MainOvt)
				{
					File.Create(ProjectDir + "\\overlay\\main_" + vv.Id.ToString("X4") + ".bin").Close();
					File.WriteAllBytes(ProjectDir + "\\overlay\\main_" + vv.Id.ToString("X4") + ".bin", NDSFile.FileData[vv.FileId]);
				}
				foreach (var vv in NDSFile.SubOvt)
				{
					File.Create(ProjectDir + "\\overlay\\sub_" + vv.Id.ToString("X4") + ".bin").Close();
					File.WriteAllBytes(ProjectDir + "\\overlay\\sub_" + vv.Id.ToString("X4") + ".bin", NDSFile.FileData[vv.FileId]);
				}

				File.Create(ProjectDir + "\\arm9.bin").Close();
				File.WriteAllBytes(ProjectDir + "\\arm9.bin", NDSFile.MainRom);
				File.Create(ProjectDir + "\\arm7.bin").Close();
				File.WriteAllBytes(ProjectDir + "\\arm7.bin", NDSFile.SubRom);

				ProjectFile.RomInfo = new NDSProjectFile.NDSRomInfo(NDSFile);
				Save();
				return true;
			}
			return false;
		}

		public override void Build()
		{
			Nitro.NDS n = new Nitro.NDS();
			n.Header = ProjectFile.RomInfo.Header;
			n.StaticFooter = ProjectFile.RomInfo.NitroFooter;
			n.MainOvt = ProjectFile.RomInfo.ARM9Ovt;
			n.SubOvt = ProjectFile.RomInfo.ARM7Ovt;
			n.Banner = ProjectFile.RomInfo.Banner;
			n.RSASignature = ProjectFile.RomInfo.RSASignature;
			n.Fnt = new Nitro.NDS.RomFNT();
			n.Fat = new Nitro.FileAllocationEntry[n.MainOvt.Length + n.SubOvt.Length];
			n.FileData = new byte[n.MainOvt.Length + n.SubOvt.Length][];
			uint fid = 0;
			foreach (var vv in n.MainOvt)
			{
				vv.FileId = fid;
				n.Fat[fid] = new Nitro.FileAllocationEntry(0, 0);
				n.FileData[fid] = File.ReadAllBytes(ProjectDir + "\\overlay\\main_" + vv.Id.ToString("X4") + ".bin");
				fid++;
			}
			foreach (var vv in n.SubOvt)
			{
				vv.FileId = fid;
				n.Fat[fid] = new Nitro.FileAllocationEntry(0, 0);
				n.FileData[fid] = File.ReadAllBytes(ProjectDir + "\\overlay\\sub_" + vv.Id.ToString("X4") + ".bin");
				fid++;
			}
			n.MainRom = File.ReadAllBytes(ProjectDir + "\\arm9.bin");
			n.SubRom = File.ReadAllBytes(ProjectDir + "\\arm7.bin");
			n.FromFileSystem(SFSDirectory.FromDirectory(ProjectDir + "\\data"));
			byte[] data = n.Write();
			File.Create(ProjectDir + "\\" + ProjectFile.ProjectName + ".nds").Close();
			File.WriteAllBytes(ProjectDir + "\\" + ProjectFile.ProjectName + ".nds",  data);
		}

		public override TreeNode[] GetProjectTree()
		{
			List<TreeNode> Nodes = new List<TreeNode>();
			var dataNode = new TreeNode("data", 1, 1);
			Nodes.Add(dataNode);
			PopulateTreeNodeFromDisk(new DirectoryInfo(ProjectDir + "\\data"), dataNode);
			var overlayNode = new TreeNode("overlay", 1, 1);
			Nodes.Add(overlayNode);
			PopulateTreeNodeFromDisk(new DirectoryInfo(ProjectDir + "\\overlay"), overlayNode);
			Nodes.Add(new TreeNode("arm9.bin"));
			Nodes.Add(new TreeNode("arm7.bin"));
			return Nodes.ToArray();
		}

		private void PopulateTreeNodeFromDisk(DirectoryInfo Dir, TreeNode Dst)
		{
			foreach (var v in Dir.EnumerateDirectories())
			{
				var t = new TreeNode(v.Name, 1, 1);
				Dst.Nodes.Add(t);
				PopulateTreeNodeFromDisk(v, t);
			}
			foreach (var v in Dir.EnumerateFiles())
			{
				Dst.Nodes.Add(v.Name);
			}
		}

		public class NDSProjectIdentifier : ProjectIdentifier
		{
			public override string GetProjectDescription()
			{
				return "NDS Rom Project";
			}
		}

		public class NDSProjectFile : ProjectFile
		{
			public NDSRomInfo RomInfo;
			public class NDSRomInfo
			{
				public NDSRomInfo() { }
				public NDSRomInfo(Nitro.NDS Rom)
				{
					Header = Rom.Header;
					NitroFooter = Rom.StaticFooter;
					ARM9Ovt = Rom.MainOvt;
					ARM7Ovt = Rom.SubOvt;
					Banner = Rom.Banner;
					RSASignature = Rom.RSASignature;
				}
				public Nitro.NDS.RomHeader Header;
				public Nitro.NDS.NitroFooter NitroFooter;
				public Nitro.NDS.RomOVT[] ARM9Ovt;
				public Nitro.NDS.RomOVT[] ARM7Ovt;
				public Nitro.NDS.RomBanner Banner;
				public byte[] RSASignature;
			}
		}
	}
}
