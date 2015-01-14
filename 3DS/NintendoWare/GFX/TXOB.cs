using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.Files;
using _3DS.GPU;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.GFX
{
	public class TXOB
	{
		public TXOB()
		{
			Signature = "TXOB";
			Revision = 0x5000000;
			Name = "";
		}
		public TXOB(EndianBinaryReader er)
		{
			Type = er.ReadUInt32();
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "TXOB") throw new SignatureNotCorrectException(Signature, "TXOB", er.BaseStream.Position);
			Revision = er.ReadUInt32();
			NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			Unknown2 = er.ReadUInt32();
			Unknown3 = er.ReadUInt32();

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = NameOffset;
			Name = er.ReadStringNT(Encoding.ASCII);
			er.BaseStream.Position = curpos;
		}
		public virtual void Write(EndianBinaryWriter er, CGFXWriterContext c)
		{
			er.Write(Type);
			er.Write(Signature, Encoding.ASCII, false);
			er.Write(Revision);
			c.WriteStringReference(Name, er);
			er.Write(Unknown2);
			er.Write(Unknown3);
		}
		public UInt32 Type;
		public String Signature;
		public UInt32 Revision;
		public UInt32 NameOffset;
		public UInt32 Unknown2;
		public UInt32 Unknown3;

		public String Name;
		public static TXOB FromStream(EndianBinaryReader er)
		{
			uint type = er.ReadUInt32();
			er.BaseStream.Position -= 4;
			//Image Texture = 0x20000011
			//Cube Texture = 0x20000009
			//Reference Texture = 0x20000004
			//Procedural Texture = 0x20000002
			//Shadow Texture = 0x20000021
			switch (type)
			{
				case 0x20000004://reference texture
					return new ReferenceTexture(er);
				case 0x20000011:
					return new ImageTextureCtr(er);
			}
			return new TXOB(er);
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class ReferenceTexture : TXOB
	{
		public ReferenceTexture(String RefTex)
			: base()
		{
			Type = 0x20000004;
			LinkedTextureName = RefTex;
		}
		public ReferenceTexture(EndianBinaryReader er)
			: base(er)
		{
			LinkedTextureNameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			LinkedTextureOffset = er.ReadUInt32();
			if (LinkedTextureOffset != 0) LinkedTextureOffset += (UInt32)er.BaseStream.Position - 4;

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = LinkedTextureNameOffset;
			LinkedTextureName = er.ReadStringNT(Encoding.ASCII);
			er.BaseStream.Position = curpos;
		}
		public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
		{
			base.Write(er, c);
			c.WriteStringReference(LinkedTextureName, er);
			if (LinkedTextureOffset == 0) er.Write((uint)0);
			else er.Write((uint)0);//TODO!
		}
		public UInt32 LinkedTextureNameOffset;
		public UInt32 LinkedTextureOffset;

		public String LinkedTextureName;
	}

	public class PixelBasedTexture : TXOB
	{
		protected static readonly uint[] GLFormats =
		{
			0x6752, 0x6754, 0x6752, 0x6754, 0x6752, 0x6758, 0x6759, 0x6757, 0x6756, 0x6758, 0x6757, 0x6756, 0x675A, 0x675B
		};

		protected static readonly uint[] GLTypes =
		{
			0x1401, 0x1401, 0x8034, 0x8363, 0x8033, 0x1401, 0x1401, 0x1401, 0x1401, 0x6760, 0x6761 , 0x6761 , 0, 0
		};

		public PixelBasedTexture()
			: base()
		{

		}
		public PixelBasedTexture(EndianBinaryReader er)
			: base(er)
		{
			Height = er.ReadUInt32();
			Width = er.ReadUInt32();
			GLFormat = er.ReadUInt32();
			GLType = er.ReadUInt32();
			NrLevels = er.ReadUInt32();
			TextureObject = er.ReadUInt32();
			LocationFlag = er.ReadUInt32();
			HWFormat = (Textures.ImageFormat)er.ReadUInt32();
		}
		public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
		{
			base.Write(er, c);
			er.Write(Height);
			er.Write(Width);
			er.Write(GLFormat);
			er.Write(GLType);
			er.Write(NrLevels);
			er.Write(TextureObject);
			er.Write(LocationFlag);
			er.Write((uint)HWFormat);
		}
		public UInt32 Height;
		public UInt32 Width;
		public UInt32 GLFormat;
		public UInt32 GLType;
		public UInt32 NrLevels;//mipmaps
		public UInt32 TextureObject;
		public UInt32 LocationFlag;
		public Textures.ImageFormat HWFormat;
	}

	public class ImageTextureCtr : PixelBasedTexture
	{
		public ImageTextureCtr(String Name, Bitmap Tex, Textures.ImageFormat Format)
			: base()
		{
			Type = 0x20000011;
			this.Name = Name;
			Width = (uint)Tex.Width;
			Height = (uint)Tex.Height;
			HWFormat = Format;
			GLFormat = GLFormats[(int)Format];
			GLType = GLTypes[(int)Format];
			NrLevels = 1;
			TextureImage = new PixelBasedImageCtr(Tex, Format);
		}
		public ImageTextureCtr(EndianBinaryReader er)
			: base(er)
		{
			TextureImageOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = TextureImageOffset;
			TextureImage = new PixelBasedImageCtr(er);
			er.BaseStream.Position = curpos;
		}
		public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
		{
			base.Write(er, c);
			if (TextureImage != null) er.Write((uint)4);
			else er.Write((uint)0);
			if (TextureImage != null) TextureImage.Write(er, c);
		}
		public UInt32 TextureImageOffset;

		public PixelBasedImageCtr TextureImage;
		public class PixelBasedImageCtr
		{
			public PixelBasedImageCtr(Bitmap Tex, Textures.ImageFormat Format)
			{
				Width = (uint)Tex.Width;
				Height = (uint)Tex.Height;
				BitsPerPixel = (uint)Textures.GetBpp(Format);
				Data = Textures.FromBitmap(Tex, Format);
				DataSize = (uint)Data.Length;
			}
			public PixelBasedImageCtr(EndianBinaryReader er)
			{
				Height = er.ReadUInt32();
				Width = er.ReadUInt32();
				DataSize = er.ReadUInt32();
				DataOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				DynamicAllocator = er.ReadUInt32();
				BitsPerPixel = er.ReadUInt32();
				LocationAddress = er.ReadUInt32();
				MemoryAddress = er.ReadUInt32();

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = DataOffset;
				Data = er.ReadBytes((int)DataSize);
				er.BaseStream.Position = curpos;
			}
			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				er.Write(Height);
				er.Write(Width);
				er.Write(DataSize);
				c.WriteDataReference(Data, er);
				er.Write(DynamicAllocator);
				er.Write(BitsPerPixel);
				er.Write(LocationAddress);
				er.Write(MemoryAddress);
			}
			public UInt32 Height;
			public UInt32 Width;
			public UInt32 DataSize;
			public UInt32 DataOffset;
			public UInt32 DynamicAllocator;
			public UInt32 BitsPerPixel;
			public UInt32 LocationAddress;
			public UInt32 MemoryAddress;

			public byte[] Data;
		}

		public Bitmap GetBitmap(int Level = 0)
		{
			int l = Level;
			uint w = Width;
			uint h = Height;
			int bpp = GPU.Textures.GetBpp(HWFormat);
			int offset = 0;
			while (l > 0)
			{
				offset += (int)(w * h * bpp / 8);
				w /= 2;
				h /= 2;
				l--;
			}
			return GPU.Textures.ToBitmap(TextureImage.Data, offset, (int)w, (int)h, HWFormat);
		}
	}
}
