using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using _3DS.UI;

namespace _3DS
{
	public class ThemeFile : FileFormat<ThemeFile.ThemeIdentifier>, IViewable, IWriteable
	{
		public Color topSolidColor;
		public Color bottomSolidColor;
		public ThemeFile(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				header = new ThemeHeader(er);

				topHeight = 256;
				bottomHeight = 256;

				switch (header.topScreenDrawType)
				{
					case 1://solid color
						er.BaseStream.Position = header.topScreenSolidColorDataOffset;//7 bytes here, first 4 is rgba, other 3 unknown (possible overlay colour ??)
						Byte r = er.ReadByte();
						Byte g = er.ReadByte();
						Byte b = er.ReadByte();
						Byte a1 = er.ReadByte();//alpha makes the colours appear lighter because the 3ds does blending
						topSolidColor = Color.FromArgb(a1, r, g, b);
						break;
					case 3://texture
						switch (header.topScreenFrameType)
						{
							case 0://tex1
							case 3://tex1
								topWidth = 1024;
								topClampWidth = 1008;
								topClampHeight = 240;
								break;
							case 1://tex0
								topWidth = 512;
								topClampWidth = 412;
								topClampHeight = 240;
								break;
						}
						break;
				}

				switch (header.bottomScreenDrawType)
				{
					case 1://solid color
						er.BaseStream.Position = header.bottomScreenSolidOrTextureOffset;//7 bytes here, first 4 is rgba, other 3 unknown (possible overlay colour ??)
						Byte r = er.ReadByte();
						Byte g = er.ReadByte();
						Byte b = er.ReadByte();
						Byte a1 = er.ReadByte();//alpha makes the colours appear lighter because the 3ds does blending

						bottomSolidColor = Color.FromArgb(a1, r, g, b);
						break;

					case 3://texture
						switch (header.bottomScreenFrameType)
						{
							case 0://tex4
							case 3://tex4
								bottomWidth = 1024;
								bottomClampWidth = 1008;
								bottomClampHeight = 240;
								break;

							case 1://tex2
								bottomWidth = 512;
								bottomClampWidth = 320;
								bottomClampHeight = 240;
								break;

							case 2://tex3
								bottomWidth = 1024;
								bottomClampWidth = 960;
								bottomClampHeight = 240;
								break;

						}
						break;
				}
				er.BaseStream.Position = header.topScreenTextureOffset;
				topScreenTexture = er.ReadBytes((topHeight * topWidth) * 2);
				er.BaseStream.Position = header.bottomScreenSolidOrTextureOffset;
				bottomScreenTexture = er.ReadBytes((bottomHeight * bottomWidth) * 2);

				er.BaseStream.Position = header.FolderOpenedTexOffset;
				openFolderTexture = er.ReadBytes((folderWidth * folderHeight) * 4);
				er.BaseStream.Position = header.FolderClosedTexOffset;
				closedFolderTexture = er.ReadBytes((folderWidth * folderHeight) * 4);

				er.BaseStream.Position = header.iconBorder48pxOffset;
				iconBorder48pxTexture = er.ReadBytes((iconBorder48pxWidth * iconBorder48pxHeight) * 4);
				er.BaseStream.Position = header.iconBorder24pxOffset;
				iconBorder24pxTexture = er.ReadBytes((iconBorder24pxWidth * iconBorder24pxHeight) * 4);
			}
			finally
			{
				er.Close();
			}
		}

		public string GetSaveDefaultFileFilter()
		{
			return "3DS System Menu Theme (body_LZ.bin)|body_LZ.bin";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);

			er.BaseStream.Position = 0xD0;//just after header - normal start of data for theme files

			header.topScreenTextureOffset = (uint)er.BaseStream.Position;
			er.Write(topScreenTexture, 0, topScreenTexture.Length);

			header.bottomScreenSolidOrTextureOffset = (uint)er.BaseStream.Position;
			er.Write(bottomScreenTexture, 0, bottomScreenTexture.Length);

			header.FolderOpenedTexOffset = (uint)er.BaseStream.Position;
			er.Write(openFolderTexture, 0, openFolderTexture.Length);

			header.FolderClosedTexOffset = (uint)er.BaseStream.Position;
			er.Write(closedFolderTexture, 0, closedFolderTexture.Length);

			header.iconBorder48pxOffset = (uint)er.BaseStream.Position;
			er.Write(iconBorder48pxTexture, 0, iconBorder48pxTexture.Length);

			header.iconBorder24pxOffset = (uint)er.BaseStream.Position;
			er.Write(iconBorder24pxTexture, 0, iconBorder24pxTexture.Length);

			header.Write(er);

			return m.ToArray();
		}

		public int iconBorder48pxWidth = 64;
		public int iconBorder48pxHeight = 128;
		public byte[] iconBorder48pxTexture;
		public Bitmap GetIconBorder48px(bool clamp)
		{
			if (clamp)
				return GPU.Textures.ToBitmap(iconBorder48pxTexture, 36, 72, GPU.Textures.ImageFormat.RGB8, false);
			return GPU.Textures.ToBitmap(iconBorder48pxTexture, iconBorder48pxWidth, iconBorder48pxHeight, GPU.Textures.ImageFormat.RGB8, true);
		}
		public int iconBorder24pxWidth = 32;
		public int iconBorder24pxHeight = 64;
		public byte[] iconBorder24pxTexture;
		public Bitmap GetIconBorder24px(bool clamp)
		{
			if (clamp)
				return GPU.Textures.ToBitmap(iconBorder24pxTexture, 25, 50, GPU.Textures.ImageFormat.RGB8, false);
			return GPU.Textures.ToBitmap(iconBorder24pxTexture, iconBorder24pxWidth, iconBorder24pxHeight, GPU.Textures.ImageFormat.RGB8, true);
		}


		public int folderWidth = 128;
		public int folderHeight = 64;
		public byte[] openFolderTexture;
		public byte[] closedFolderTexture;
		public Bitmap GetOpenFolderTexture(bool clamp)
		{
			if (clamp)
				return GPU.Textures.ToBitmap(openFolderTexture, 82, 64, GPU.Textures.ImageFormat.RGB8, false);
			return GPU.Textures.ToBitmap(openFolderTexture, folderWidth, folderHeight, GPU.Textures.ImageFormat.RGB8, true);
		}
		public Bitmap GetClosedFolderTexture(bool clamp)
		{
			if (clamp)
				return GPU.Textures.ToBitmap(closedFolderTexture, 74, 64, GPU.Textures.ImageFormat.RGB8, false);
			return GPU.Textures.ToBitmap(closedFolderTexture, folderWidth, folderHeight, GPU.Textures.ImageFormat.RGB8, true);
		}


		public int bottomWidth;
		public int bottomHeight;
		public byte[] bottomScreenTexture;
		public int bottomClampWidth;
		public int bottomClampHeight;

		public Bitmap GetBottomTexture(bool clamp)
		{
			if (clamp)
				return GPU.Textures.ToBitmap(bottomScreenTexture, bottomClampWidth, bottomClampHeight, GPU.Textures.ImageFormat.RGB565, false);
			return GPU.Textures.ToBitmap(bottomScreenTexture, bottomWidth, bottomHeight, GPU.Textures.ImageFormat.RGB565, true);
		}

		public int topWidth;
		public int topHeight;
		public byte[] topScreenTexture;
		public int topClampWidth;
		public int topClampHeight;
		public Bitmap GetTopTexture(bool clamp)
		{
			if (clamp)
				return GPU.Textures.ToBitmap(topScreenTexture, topClampWidth, topClampHeight, GPU.Textures.ImageFormat.RGB565, false);
			return GPU.Textures.ToBitmap(topScreenTexture, topWidth, topHeight, GPU.Textures.ImageFormat.RGB565, true);
		}

		public System.Windows.Forms.Form GetDialog()
		{
			return new ThemeViewer(this);
		}

		public AudioSection audioSection;
		public class AudioSection
		{
			public AudioSection(EndianBinaryReader er)
			{

			}
		}

		public ThemeHeader header;
		public class ThemeHeader
		{
			public ThemeHeader(EndianBinaryReader er)
			{
				//offset 0x0
				Version = er.ReadUInt32();

				Unknown1 = er.ReadByte();
				UseBGMusic = er.ReadByte() == 1;
				Padding = er.ReadBytes(6);

				//offset 0xC
				topScreenDrawType = er.ReadUInt32();// 0 = none, 1 = solid colour, 2 = extension of val1, 3 = texture
				topScreenFrameType = er.ReadUInt32();//0 = solid colour(only when draw type is 1 else uses tex 0), 1 = texture0, 3 = texture1
				topScreenSolidColorDataOffset = er.ReadUInt32();
				topScreenTextureOffset = er.ReadUInt32();
				topScreenAdditionalTextureOffset = er.ReadUInt32();//used with draw type val2, optional when using draw type val2

				//offset 0x20
				bottomScreenDrawType = er.ReadUInt32();
				bottomScreenFrameType = er.ReadUInt32();
				bottomScreenSolidOrTextureOffset = er.ReadUInt32();

				//offset 0x2C
				UseSelectorColor = er.ReadUInt32() == 1;
				SelectorColorBlockOffset = er.ReadUInt32();//0xC length

				//offset 0x34
				UseFolderColor = er.ReadUInt32() == 1;
				FolderColorBlockOffset = er.ReadUInt32();//0xC length

				//offset 0x3C
				UseFolderTex = er.ReadUInt32() == 1;
				FolderClosedTexOffset = er.ReadUInt32();//texture 6
				FolderOpenedTexOffset = er.ReadUInt32();//texture 7

				//offset 0x48
				useUnknownBlock2 = er.ReadUInt32() == 1;
				unknownBlock2Offset = er.ReadUInt32();//0xD length

				//offset 0x50
				useIconBorders = er.ReadUInt32() == 1;
				iconBorder48pxOffset = er.ReadUInt32();
				iconBorder24pxOffset = er.ReadUInt32();

				//offset 0x4C
				useUnknownBlock3 = er.ReadUInt32() == 1;
				unknownBlock3Offset = er.ReadUInt32();//0xD length

				useUnknownBlock4 = er.ReadUInt32() == 1;
				unknownBlock4Offset = er.ReadUInt32();//0x9 length

				useUnknownBlock5and6 = er.ReadUInt32() == 1;
				unknownBlock5Offset = er.ReadUInt32();//0x20 length
				unknownBlock6Offset = er.ReadUInt32();//0x20 length

				useUnknownBlock7 = er.ReadUInt32() == 1;
				unknownBlock7Offset = er.ReadUInt32();//0xD length

				useUnknownBlock8 = er.ReadUInt32() == 1;
				unknownBlock8Offset = er.ReadUInt32();//0xD length

				useUnknownBlock9 = er.ReadUInt32() == 1;
				unknownBlock9Offset = er.ReadUInt32();//0x9 length

				useUnknownBlock10 = er.ReadUInt32() == 1;
				unknownBlock10Offset = er.ReadUInt32();//0xD length

				useUnknownBlock11 = er.ReadUInt32() == 1;
				unknownBlock11Offset = er.ReadUInt32();//0x20 length

				useUnknownBlock12 = er.ReadUInt32() == 1;
				unknownBlock12Offset = er.ReadUInt32();//0x15 length

				useUnknownBlock13 = er.ReadUInt32() == 1;
				unknownBlock13Offset = er.ReadUInt32();//0xC length

				useUnknownBlock14 = er.ReadUInt32() == 1;
				unknownBlock14Offset = er.ReadUInt32();//0x6 length

				useAudioSection = er.ReadUInt32() == 1;
				audioSectionSize = er.ReadUInt32();
				audioSectionOffset = er.ReadUInt32();
			}

			public void Write(EndianBinaryWriter er)
			{
				er.BaseStream.Position = 0;
				//write header data
				er.Write(Version);

				er.Write(Unknown1);
				er.Write((byte)(UseBGMusic ? 1 : 0));
				er.Write(Padding, 0, 6);

				er.Write(topScreenDrawType);
				er.Write(topScreenFrameType);
				er.Write(topScreenSolidColorDataOffset);
				er.Write(topScreenTextureOffset);
				er.Write(topScreenAdditionalTextureOffset);


				er.Write(bottomScreenDrawType);
				er.Write(bottomScreenFrameType);
				er.Write(bottomScreenSolidOrTextureOffset);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write(UseFolderTex ? 1u : 0u);//TODO write 0 instead or somethign when this is disabled?
				er.Write(FolderClosedTexOffset);
				er.Write(FolderOpenedTexOffset);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write(useIconBorders ? 1u : 0u);//TODO write 0 instead or somethign when this is disabled?
				er.Write(iconBorder48pxOffset);
				er.Write(iconBorder24pxOffset);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);

				er.Write((UInt32)0);
				er.Write((UInt32)0);
				er.Write((UInt32)0);
			}

			public UInt32 Version;
			public Byte Unknown1;
			public bool UseBGMusic;
			public Byte[] Padding;//6

			public UInt32 topScreenDrawType;
			public UInt32 topScreenFrameType;
			public UInt32 topScreenSolidColorDataOffset;
			public UInt32 topScreenTextureOffset;
			public UInt32 topScreenAdditionalTextureOffset;

			public UInt32 bottomScreenDrawType;
			public UInt32 bottomScreenFrameType;
			public UInt32 bottomScreenSolidOrTextureOffset;

			public bool UseSelectorColor;
			public UInt32 SelectorColorBlockOffset;

			public bool UseFolderColor;
			public UInt32 FolderColorBlockOffset;

			public bool UseFolderTex;//6 and 7
			public UInt32 FolderClosedTexOffset;
			public UInt32 FolderOpenedTexOffset;

			public bool useUnknownBlock2;
			public UInt32 unknownBlock2Offset;

			public bool useIconBorders;
			public UInt32 iconBorder48pxOffset;
			public UInt32 iconBorder24pxOffset;

			public bool useUnknownBlock3;
			public UInt32 unknownBlock3Offset;

			public bool useUnknownBlock4;
			public UInt32 unknownBlock4Offset;

			public bool useUnknownBlock5and6;
			public UInt32 unknownBlock5Offset;
			public UInt32 unknownBlock6Offset;

			public bool useUnknownBlock7;
			public UInt32 unknownBlock7Offset;

			public bool useUnknownBlock8;
			public UInt32 unknownBlock8Offset;

			public bool useUnknownBlock9;
			public UInt32 unknownBlock9Offset;

			public bool useUnknownBlock10;
			public UInt32 unknownBlock10Offset;

			public bool useUnknownBlock11;
			public UInt32 unknownBlock11Offset;

			public bool useUnknownBlock12;
			public UInt32 unknownBlock12Offset;

			public bool useUnknownBlock13;
			public UInt32 unknownBlock13Offset;

			public bool useUnknownBlock14;
			public UInt32 unknownBlock14Offset;


			public Boolean useAudioSection;
			public UInt32 audioSectionSize;
			public UInt32 audioSectionOffset;
		}

		public class ThemeIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "3DS Themes";
			}

			public override string GetFileDescription()
			{
				return "3DS System Menu Theme (body_LZ.bin)";
			}

			public override string GetFileFilter()
			{
				return "3DS System Menu Theme (body_LZ.bin)|body_LZ.bin";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Name.Equals("body_LZ.bin"))
					return FormatMatch.Content;
				else
					return FormatMatch.No;
			}

		}

	}



}
