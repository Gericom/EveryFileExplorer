using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Windows.Forms;
using LibEveryFileExplorer._3D;
using CommonFiles;
using Tao.OpenGl;
using LibEveryFileExplorer.Collections;

namespace _3DS.NintendoWare.GFX
{
	public class CGFX : FileFormat<CGFX.CGFXIdentifier>, IFileCreatable, IViewable, IWriteable
	{
		public CGFX()
		{
			Header = new CGFXHeader();
			Data = new DATA();
		}

		public CGFX(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CGFXHeader(er);
				this.Data = new DATA(er);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new UI.CGFXViewer(this);
		}

		public string GetSaveDefaultFileFilter()
		{
            return "CTR Graphics Resource (*.bcres;*.bcmdl)|*.bcres;*.bcmdl";
		}

		public byte[] Write()
		{
            //MessageBox.Show("CGFX saving is experimental! A couple of sections (like animations) are lost while saving!");
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.NrBlocks = 1;
			Header.Write(er);
			CGFXWriterContext c = new CGFXWriterContext();
			Data.Write(er, c);
			if (c.DoWriteIMAGBlock())
			{
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = 0x10;
				er.Write((uint)2);
				er.BaseStream.Position = curpos;
				c.WriteIMAGBlock(er);
			}
			long curpos2 = er.BaseStream.Position;
			er.BaseStream.Position = 0xC;
			er.Write((uint)(curpos2));
			er.BaseStream.Position = curpos2;

			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public bool CreateFromFile()
		{
			System.Windows.Forms.OpenFileDialog f = new System.Windows.Forms.OpenFileDialog();
			f.Filter = OBJ.Identifier.GetFileFilter();
			if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& f.FileName.Length > 0)
			{
				UI.CGFXGenDialog d = new UI.CGFXGenDialog();
				d.ShowDialog();
				CGFXGenerator.FromOBJ(this, f.FileName, d.ModelName);
				return true;
			}
			return false;
		}

		public CGFXHeader Header;
		public class CGFXHeader
		{
			public CGFXHeader()
			{
				Signature = "CGFX";
				Endianness = 0xFEFF;
				HeaderSize = 0x14;
				Version = 0x5000000;
			}
			public CGFXHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CGFX") throw new SignatureNotCorrectException(Signature, "CGFX", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				FileSize = er.ReadUInt32();
				NrBlocks = er.ReadUInt32();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Endianness);
				er.Write(HeaderSize);
				er.Write(Version);
				er.Write((uint)0);
				er.Write(NrBlocks);
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 FileSize;
			public UInt32 NrBlocks;
		}
		public DATA Data;
		public class DATA
		{
			public DATA()
			{
				Signature = "DATA";
				DictionaryEntries = new DictionaryInfo[16];
				Dictionaries = new DICT[16];
			}
			public DATA(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				DictionaryEntries = new DictionaryInfo[16];
				for (int i = 0; i < 16; i++)
				{
					DictionaryEntries[i] = new DictionaryInfo(er);
				}
				Dictionaries = new DICT[16];
				for (int i = 0; i < 16; i++)
				{
					if (i == 15 && DictionaryEntries[i].NrItems == 0x54434944)
					{
						DictionaryEntries[i].NrItems = 0;
						DictionaryEntries[i].Offset = 0;
					}
					if (DictionaryEntries[i].Offset != 0)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = DictionaryEntries[i].Offset;
						Dictionaries[i] = new DICT(er);
						er.BaseStream.Position = curpos;
					}
					else Dictionaries[i] = null;
				}

				if (Dictionaries[0] != null)
				{
					Models = new CMDL[Dictionaries[0].Count];
					for (int i = 0; i < Dictionaries[0].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[0][i].DataOffset;
						Models[i] = new CMDL(er);
						er.BaseStream.Position = curpos;
					}
				}

				if (Dictionaries[1] != null)
				{
					Textures = new TXOB[Dictionaries[1].Count];
					for (int i = 0; i < Dictionaries[1].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[1][i].DataOffset;
						Textures[i] = TXOB.FromStream(er);//new TXOB(er);
						er.BaseStream.Position = curpos;
					}
				}
				if (Dictionaries[9] != null)
				{
					SkeletonAnimations = new CANM[Dictionaries[9].Count];
					for (int i = 0; i < Dictionaries[9].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[9][i].DataOffset;
						SkeletonAnimations[i] = new CANM(er);
						er.BaseStream.Position = curpos;
					}
				}
				if (Dictionaries[10] != null)
				{
					MaterialAnimations = new CANM[Dictionaries[10].Count];
					for (int i = 0; i < Dictionaries[10].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[10][i].DataOffset;
						MaterialAnimations[i] = new CANM(er);
						er.BaseStream.Position = curpos;
					}
				}
				if (Dictionaries[11] != null)
				{
					VisibilityAnimations = new CANM[Dictionaries[11].Count];
					for (int i = 0; i < Dictionaries[11].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[11][i].DataOffset;
						VisibilityAnimations[i] = new CANM(er);
						er.BaseStream.Position = curpos;
					}
				}
			}
			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				long basepos = er.BaseStream.Position;
				er.Write(Signature, Encoding.ASCII, false);
				er.Write((uint)0);
				for (int i = 0; i < 16; i++)
				{
                    if (Dictionaries[i] != null && (i == 0 || i == 1))
					{
						//if (i != 0 && i != 1) throw new NotImplementedException();
						er.Write((uint)Dictionaries[i].Count);
						er.Write((uint)0);//dictoffset
					}
					else
					{
						er.Write((uint)0);
						er.Write((uint)0);
					}
				}
				long[] dictoffsets = new long[16];
				for (int i = 0; i < 16; i++)
				{
					if (Dictionaries[i] != null && (i == 0 || i == 1))
					{
						dictoffsets[i] = er.BaseStream.Position;
						er.BaseStream.Position = basepos + 8 + i * 8 + 4;
						er.Write((uint)(dictoffsets[i] - (basepos + 8 + i * 8 + 4)));
						er.BaseStream.Position = dictoffsets[i];
						Dictionaries[i].Write(er, c);
					}
				}
				if (Dictionaries[0] != null)
				{
					for (int i = 0; i < Dictionaries[0].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						long bpos = er.BaseStream.Position = dictoffsets[0] + 0x1C + i * 0x10 + 0xC;
						er.Write((uint)(curpos - bpos));
						er.BaseStream.Position = curpos;
						Models[i].Write(er, c);
					}
				}
				if (Dictionaries[1] != null)
				{
					for (int i = 0; i < Dictionaries[1].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						long bpos = er.BaseStream.Position = dictoffsets[1] + 0x1C + i * 0x10 + 0xC;
						er.Write((uint)(curpos - bpos));
						er.BaseStream.Position = curpos;
						Textures[i].Write(er, c);
					}
				}
				c.WriteStringTable(er);
				if (c.DoWriteIMAGBlock())
				{
					int length = c.GetIMAGBlockSize();
					while (((er.BaseStream.Position + length) % 64) != 0) er.Write((byte)0);
				}
				long curpos2 = er.BaseStream.Position;
				er.BaseStream.Position = basepos + 4;
				er.Write((uint)(curpos2 - basepos));
				er.BaseStream.Position = curpos2;
			}

			public String Signature;
			public UInt32 SectionSize;
			public DictionaryInfo[] DictionaryEntries;//x15
			public class DictionaryInfo
			{
				public DictionaryInfo(EndianBinaryReader er)
				{
					NrItems = er.ReadUInt32();
					long pos = er.BaseStream.Position;
					Offset = er.ReadUInt32();
					if (Offset != 0) Offset += (UInt32)pos;
				}
				public UInt32 NrItems;
				public UInt32 Offset;
			}
			public DICT[] Dictionaries;
			//0x0 - CMDL
			//0x1 - TXOB
			//0x9 - CANM (Skeleton Animation)
			//0xA - CANM (Material Animation)

			public CMDL[] Models;
			public TXOB[] Textures;
			public CANM[] SkeletonAnimations;
			public CANM[] MaterialAnimations;
			public CANM[] VisibilityAnimations;
		}

		public class CGFXIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Models;
			}

			public override string GetFileDescription()
			{
				return "CTR Graphics (CGFX)";
			}

			public override string GetFileFilter()
			{
				return "CTR Graphics (*.cgfx, *.bcenv, *.bclts *.bctex, *.bccam, *.bcfog, *.bclgt, *.bcmata, *.bcmcla, *.bcmdl, *.bcptl, *.bcres, *.bcsdr, *.bcskla)|*.cgfx;*.bcenv;*.bclts;*.bctex;*.bccam;*.bcfog;*.bclgt;*.bcmata;*.bcmcla;*.bcmdl;*.bcptl;*.bcres;*.bcsdr;*.bcskla";
			}

			public override Bitmap GetIcon()
			{
				return Resource.leaf;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'G' && File.Data[2] == 'F' && File.Data[3] == 'X') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
