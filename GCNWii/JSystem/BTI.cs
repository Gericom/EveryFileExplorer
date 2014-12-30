using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using GCNWii.GPU;
using System.IO;
using System.Windows.Forms;
using GCNWii.UI;

namespace GCNWii.JSystem
{
	public class BTI : FileFormat<BTI.BTIIdentifier>, IConvertable, IViewable
	{
		public BTI(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
			try
			{
				Header = new BTIHeader(er);
				er.BaseStream.Position = Header.TextureOffset;
				int len = (int)(er.BaseStream.Length - Header.TextureOffset);
				if (Header.PaletteOffset != 0) len = (int)(Header.PaletteOffset - Header.TextureOffset);
				Texture = er.ReadBytes(len);
				if (Header.PaletteOffset != 0)
				{
					er.BaseStream.Position = Header.PaletteOffset;
					Palette = er.ReadBytes(Header.NrPaletteEntries * 2);
				}
			}
			finally
			{
				er.Close();
			}
		}

		public string GetConversionFileFilters()
		{
			return "Portable Network Graphics (*.png)|*.png";
		}

		public bool Convert(int FilterIndex, string Path)
		{
			switch (FilterIndex)
			{
				case 0:
					File.Create(Path).Close();
					ToBitmap().Save(Path, System.Drawing.Imaging.ImageFormat.Png);
					return true;
			}
			return false;
		}

		public Form GetDialog()
		{
			return new BTIViewer(this);
		}

		public BTIHeader Header;
		public class BTIHeader
		{
			public BTIHeader(EndianBinaryReader er)
			{
				TextureFormat = (Textures.ImageFormat)er.ReadByte();
				Unknown1 = er.ReadByte();
				Width = er.ReadUInt16();
				Height = er.ReadUInt16();
				Unknown2 = er.ReadUInt16();
				Unknown3 = er.ReadByte();
				PaletteFormat = (Textures.PaletteFormat)er.ReadByte();
				NrPaletteEntries = er.ReadUInt16();
				PaletteOffset = er.ReadUInt32();
				Unknown4 = er.ReadUInt32();
				Unknown5 = er.ReadUInt16();
				Unknown6 = er.ReadUInt16();
				MipMapCount = er.ReadByte();
				Unknown7 = er.ReadByte();
				Unknown8 = er.ReadUInt16();
				TextureOffset = er.ReadUInt32();
			}
			public Textures.ImageFormat TextureFormat; //1
			public Byte Unknown1;
			public UInt16 Width;
			public UInt16 Height;
			public UInt16 Unknown2;
			public Byte Unknown3;
			public Textures.PaletteFormat PaletteFormat; //1
			public UInt16 NrPaletteEntries;
			public UInt32 PaletteOffset;
			public UInt32 Unknown4;
			public UInt16 Unknown5;
			public UInt16 Unknown6;
			public Byte MipMapCount;
			public Byte Unknown7;
			public UInt16 Unknown8;
			public UInt32 TextureOffset;
		}
		public byte[] Texture;
		public byte[] Palette;

		public Bitmap ToBitmap(int Level = 0)
		{
			int l = Level;
			uint w = Header.Width;
			uint h = Header.Height;
			int bpp = GPU.Textures.GetBpp(Header.TextureFormat);
			int offset = 0;
			while (l > 0)
			{
				offset += (int)(w * h * bpp / 8);
				w /= 2;
				h /= 2;
				l--;
			}
			return GPU.Textures.ToBitmap(Texture, offset, Palette, 0, (int)w, (int)h, Header.TextureFormat, Header.PaletteFormat);
		}

		public class BTIIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Graphics;
			}

			public override string GetFileDescription()
			{
				return "Binary Texture Image (BTI)";
			}

			public override string GetFileFilter()
			{
				return "Binary Texture Image (*.bti)|*.bti";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Name.ToLower().EndsWith(".bti")) return FormatMatch.Extension;
				return FormatMatch.No;
			}
		}
	}
}
