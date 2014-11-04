using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.Files;
using _3DS.GPU;

namespace _3DS.NintendoWare.GFX
{
	/*public class TXOB
	{
		public TXOB() { }
		public TXOB(EndianBinaryReader er)
		{
			Flags = er.ReadUInt32();
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "TXOB") throw new SignatureNotCorrectException(Signature, "TXOB", er.BaseStream.Position);
			Unknown1 = er.ReadUInt32();
			NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			Unknown2 = er.ReadUInt32();
			Unknown3 = er.ReadUInt32();
			Height = er.ReadUInt32();
			Width = er.ReadUInt32();
			GlFormat = er.ReadUInt32();
			Type = er.ReadUInt32();
			NrLevels = er.ReadUInt32();
			Unknown7 = er.ReadUInt32();
			Unknown8 = er.ReadUInt32();
			Format = (GPU.Textures.ImageFormat)er.ReadUInt32();
			Unknown9 = er.ReadUInt32();
			Height2 = er.ReadUInt32();
			Width2 = er.ReadUInt32();
			DataSize = er.ReadUInt32();
			DataOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			Unknown10 = er.ReadUInt32();
			BitsPerPixel = er.ReadUInt32();
			Unknown12 = er.ReadUInt32();

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = NameOffset;
			Name = er.ReadStringNT(Encoding.ASCII);
			er.BaseStream.Position = DataOffset;
			Data = er.ReadBytes((int)DataSize);
			er.BaseStream.Position = curpos;
		}
		public UInt32 Flags;//?
		public String Signature;
		public UInt32 Unknown1;
		public UInt32 NameOffset;
		public UInt32 Unknown2;
		public UInt32 Unknown3;
		public UInt32 Height;
		public UInt32 Width;
		public UInt32 GlFormat;
		public UInt32 Type;
		public UInt32 NrLevels;
		public UInt32 Unknown7;
		public UInt32 Unknown8;
		public GPU.Textures.ImageFormat Format;
		public UInt32 Unknown9;
		public UInt32 Height2;
		public UInt32 Width2;
		public UInt32 DataSize;
		public UInt32 DataOffset;
		public UInt32 Unknown10;
		public UInt32 BitsPerPixel;
		public UInt32 Unknown12;

		public String Name;
		public byte[] Data;

		//The flags are like this:
		//Image Texture = 0x20000011 (this structure)
		//Cube Texture = 0x20000009
		//Reference Texture = 0x20000004
		//Procedural Texture = 0x20000002
		//Shadow Texture = 0x20000021

		public Bitmap GetBitmap(int Level = 0)
		{
			int l = Level;
			uint w = Width;
			uint h = Height;
			int bpp = GPU.Textures.GetBpp(Format);
			int offset = 0;
			while (l > 0)
			{
				offset += (int)(w * h * bpp / 8);
				w /= 2;
				h /= 2;
				l--;
			}
			return GPU.Textures.ToBitmap(Data, offset, (int)w, (int)h, Format);
		}

		public override string ToString()
		{
			return Name;
		}
	}*/

	public class TXOB
	{
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
		public PixelBasedTexture(EndianBinaryReader er)
			: base(er)
		{
			Height = er.ReadUInt32();
			Width = er.ReadUInt32();
			GLFormat = er.ReadUInt32();
			Type = er.ReadUInt32();
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
			er.Write(Type);
			er.Write(NrLevels);
			er.Write(TextureObject);
			er.Write(LocationFlag);
			er.Write((uint)HWFormat);
		}
		public UInt32 Height;
		public UInt32 Width;
		public UInt32 GLFormat;
		public UInt32 Type;
		public UInt32 NrLevels;//mipmaps
		public UInt32 TextureObject;
		public UInt32 LocationFlag;
		public Textures.ImageFormat HWFormat;
	}

	public class ImageTextureCtr : PixelBasedTexture
	{
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
