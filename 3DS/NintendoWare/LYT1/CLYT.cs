using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using System.IO;
using System.Windows.Forms;
using _3DS.UI;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.LYT1
{
	public class CLYT : FileFormat<CLYT.CLYTIdentifier>, IViewable
	{
		public CLYT(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CLYTHeader(er);
				pan1 ParentPan = new pan1("FakeDummyTopPane");
				pan1 LastPan = null;
				grp1 ParentGrp = new grp1("FakeDummyTopGroup");
				grp1 LastGrp = null;
				int blocknr = 0;
				while (blocknr < Header.NrBlocks)
				{
					String sig = er.ReadString(Encoding.ASCII, 4);
					UInt32 size = er.ReadUInt32();
					er.BaseStream.Position -= 8;
					switch (sig)
					{
						case "lyt1": Layout = new lyt1(er); break;
						case "txl1": TextureList = new txl1(er); break;
						case "fnl1": FontList = new fnl1(er); break;
						case "mat1": Materials = new mat1(er); break;
						case "pan1":
							LastPan = new pan1(er);
							LastPan.Parent = ParentPan;
							ParentPan.Children.Add(LastPan);
							break;
						case "pic1":
							LastPan = new pic1(er);
							LastPan.Parent = ParentPan;
							ParentPan.Children.Add(LastPan);
							break;
						case "txt1":
							LastPan = new txt1(er);
							LastPan.Parent = ParentPan;
							ParentPan.Children.Add(LastPan);
							break;
						case "bnd1":
							LastPan = new bnd1(er);
							LastPan.Parent = ParentPan;
							ParentPan.Children.Add(LastPan);
							break;
						case "wnd1":
							LastPan = new wnd1(er);
							LastPan.Parent = ParentPan;
							ParentPan.Children.Add(LastPan);
							break;
						case "pas1":
							ParentPan = LastPan;
							er.BaseStream.Position += 8;
							break;
						case "pae1":
							ParentPan = ParentPan.Parent;
							er.BaseStream.Position += 8;
							break;
						case "grp1":
							LastGrp = new grp1(er);
							LastGrp.Parent = ParentGrp;
							ParentGrp.Children.Add(LastGrp);
							break;
						case "grs1":
							ParentGrp = LastGrp;
							er.BaseStream.Position += 8;
							break;
						case "gre1":
							ParentGrp = ParentGrp.Parent;
							er.BaseStream.Position += 8;
							break;
						case "usd1"://userdata
							er.BaseStream.Position += size;
							break;
						default:
							er.BaseStream.Position += size;
							break;
					}
					blocknr++;
				}
				RootPane = ParentPan.Children[0];
				RootPane.Parent = null;
				RootGroup = ParentGrp.Children[0];
				RootGroup.Parent = null;
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new CLYTViewer(this);
		}

		public CLYTHeader Header;
		public class CLYTHeader
		{
			public CLYTHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CLYT") throw new SignatureNotCorrectException(Signature, "CLYT", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				FileSize = er.ReadUInt32();
				NrBlocks = er.ReadUInt32();
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 FileSize;
			public UInt32 NrBlocks;
		}
		public lyt1 Layout;
		public class lyt1
		{
			public enum ScreenOriginType : uint
			{
				Classic = 0,
				Normal = 1
			}
			public lyt1(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "lyt1") throw new SignatureNotCorrectException(Signature, "lyt1", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				ScreenOrigin = (ScreenOriginType)er.ReadUInt32();
				LayoutSize = er.ReadVector2();
			}
			public String Signature;
			public UInt32 SectionSize;
			public ScreenOriginType ScreenOrigin;//u32
			public Vector2 LayoutSize;
		}
		public txl1 TextureList;
		public class txl1
		{
			public txl1(EndianBinaryReader er)
			{
				long startpos = er.BaseStream.Position;
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "txl1") throw new SignatureNotCorrectException(Signature, "txl1", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				NrTextures = er.ReadUInt32();
				long baseoffset = er.BaseStream.Position;
				TextureNameOffsets = er.ReadUInt32s((int)NrTextures);
				TextureNames = new string[NrTextures];
				for (int i = 0; i < NrTextures; i++)
				{
					er.BaseStream.Position = baseoffset + TextureNameOffsets[i];
					TextureNames[i] = er.ReadStringNT(Encoding.ASCII);
				}
				//padding
				er.BaseStream.Position = startpos + SectionSize;
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt32 NrTextures;
			public UInt32[] TextureNameOffsets;

			public String[] TextureNames;
		}
		public fnl1 FontList;
		public class fnl1
		{
			public fnl1(EndianBinaryReader er)
			{
				long startpos = er.BaseStream.Position;
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "fnl1") throw new SignatureNotCorrectException(Signature, "fnl1", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				NrFonts = er.ReadUInt32();
				long baseoffset = er.BaseStream.Position;
				FontNameOffsets = er.ReadUInt32s((int)NrFonts);
				FontNames = new string[NrFonts];
				for (int i = 0; i < NrFonts; i++)
				{
					er.BaseStream.Position = baseoffset + FontNameOffsets[i];
					FontNames[i] = er.ReadStringNT(Encoding.ASCII);
				}
				er.BaseStream.Position = startpos + SectionSize;
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt32 NrFonts;
			public UInt32[] FontNameOffsets;

			public String[] FontNames;
		}
		public mat1 Materials;
		public pan1 RootPane;
		public grp1 RootGroup;
		public class grp1
		{
			public grp1(String Name) { this.Name = Name; }
			public grp1(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "grp1") throw new SignatureNotCorrectException(Signature, "grp1", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				Name = er.ReadString(Encoding.ASCII, 0x10).Replace("\0", "");
				NrPaneReferences = er.ReadUInt32();
				PaneReferences = new string[NrPaneReferences];
				for (int i = 0; i < NrPaneReferences; i++)
				{
					PaneReferences[i] = er.ReadString(Encoding.ASCII, 0x10).Replace("\0", "");
				}
			}
			public String Signature;
			public UInt32 SectionSize;
			public String Name;
			public UInt32 NrPaneReferences;
			public String[] PaneReferences;

			public grp1 Parent = null;
			public List<grp1> Children = new List<grp1>();

			public TreeNode GetTreeNodes()
			{
				TreeNode t = new TreeNode(Name);
				t.ImageKey = t.SelectedImageKey = Signature;
				foreach (var v in Children)
				{
					t.Nodes.Add(v.GetTreeNodes());
				}
				foreach (var v in PaneReferences)
				{
					t.Nodes.Add("pan1", v);
				}
				return t;
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public class CLYTIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Layouts;
			}

			public override string GetFileDescription()
			{
				return "CTR Layout (CLYT)";
			}

			public override string GetFileFilter()
			{
				return "CTR Layout (*.bclyt)|*.bclyt";
			}

			public override Bitmap GetIcon()
			{
				return Resource.zone;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'L' && File.Data[2] == 'Y' && File.Data[3] == 'T') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
