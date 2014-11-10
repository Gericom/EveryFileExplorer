using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using NDS.UI;

namespace NDS.NitroSystem.G3D
{
	public class NSBMD : FileFormat<NSBMD.NSBMDIdentifier>, IViewable//, IWriteable
	{
		public NSBMD(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new NSBMDHeader(er);
				if (Header.NrBlocks > 0)
				{
					er.BaseStream.Position = Header.BlockOffsets[0];
					ModelSet = new MDL0(er);
				}
				if (Header.NrBlocks > 1)
				{
					er.BaseStream.Position = Header.BlockOffsets[1];
					TexPlttSet = new TEX0(er);
				}
			}
			finally
			{
				er.Close();
			}
		}

		public System.Windows.Forms.Form GetDialog()
		{
			return new NSBMDViewer(this);
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.NrBlocks = (ushort)(TexPlttSet != null ? 2 : 1);
			Header.Write(er);

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = 16;
			er.Write((UInt32)curpos);
			er.BaseStream.Position = curpos;

			ModelSet.Write(er);
			if (TexPlttSet != null)
			{
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = 20;
				er.Write((UInt32)curpos);
				er.BaseStream.Position = curpos;

				TexPlttSet.Write(er);
			}
			er.BaseStream.Position = 8;
			er.Write((UInt32)er.BaseStream.Length);
			byte[] b = m.ToArray();
			er.Close();
			return b;
		}

		public NSBMDHeader Header;
		public class NSBMDHeader
		{
			public NSBMDHeader(bool HasTEX0)
			{
				Signature = "BMD0";
				Endianness = 0xFEFF;
				Version = 2;//1;
				HeaderSize = 16;
				NrBlocks = (ushort)(HasTEX0 ? 2 : 1);
				BlockOffsets = new uint[NrBlocks];
			}
			public NSBMDHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "BMD0") throw new SignatureNotCorrectException(Signature, "BMD0", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				Version = er.ReadUInt16();
				FileSize = er.ReadUInt32();
				HeaderSize = er.ReadUInt16();
				NrBlocks = er.ReadUInt16();
				BlockOffsets = er.ReadUInt32s(NrBlocks);
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Endianness);
				er.Write(Version);
				er.Write((uint)0);
				er.Write(HeaderSize);
				er.Write(NrBlocks);
				er.Write(new uint[NrBlocks], 0, NrBlocks);
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 Version;
			public UInt32 FileSize;
			public UInt16 HeaderSize;
			public UInt16 NrBlocks;
			public UInt32[] BlockOffsets;
		}
		public MDL0 ModelSet;
		public TEX0 TexPlttSet;
		public class NSBMDIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Models;
			}

			public override string GetFileDescription()
			{
				return "Nitro System Binary Model (NSBMD)";
			}

			public override string GetFileFilter()
			{
				return "Nitro System Binary Model (*.nsbmd)|*.nsbmd";
			}

			public override Bitmap GetIcon()
			{
				return Resource.leaf;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'M' && File.Data[2] == 'D' && File.Data[3] == '0') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
