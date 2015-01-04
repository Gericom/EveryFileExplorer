using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.Collections;
using LibEveryFileExplorer.IO;
using System.Drawing.Imaging;
using LibEveryFileExplorer.GFX;
using CommonFiles;

namespace MarioKart.MK64
{
	public class MK64 : FileFormat<MK64.MK64Identifier>, IViewable
	{
		private static readonly int TrackHeaderAddress = 0x122390;
		private static readonly int TexBaseAddress = 0x641F70;

		public MK64(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
			try
			{
				Tracks = new Track[20];
				for (int i = 0; i < 20; i++)
				{
					Tracks[i] = new Track(er, i);
				}
				/*int tracknr = 0;
				string objpath = @"d:\Temp\mk64\TrackDump\" + tracknr + @"\track.obj";
				var mm = Tracks[tracknr].GetModel();
				OBJ o = mm.ToOBJ();
				o.MTLPath = Path.GetFileNameWithoutExtension(objpath) + ".mtl";
				MTL m = mm.ToMTL();
				File.Create(objpath).Close();
				File.WriteAllBytes(objpath, o.Write());
				File.Create(Path.ChangeExtension(objpath, "mtl")).Close();
				File.WriteAllBytes(Path.ChangeExtension(objpath, "mtl"), m.Write());
				Directory.CreateDirectory(Path.GetDirectoryName(objpath) + @"\Tex\");
				foreach (var mmm in mm.Materials)
				{
					if (mmm.Value.Texture != null) 
						mmm.Value.Texture.Save(Path.GetDirectoryName(objpath) + @"\Tex\" + mmm.Key + ".png");

				}*/
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new Form();
		}

		public Track[] Tracks;
		public class Track
		{
			public Track(EndianBinaryReader er, int Id)
			{
				er.BaseStream.Position = TrackHeaderAddress + Id * 0x30;
				Header = new TrackHeader(er);
				er.BaseStream.Position = Header.DLOffset;
				DL = MIO0.Decompress(er.ReadBytes((int)(Header.DLEnd - Header.DLOffset)));
				er.BaseStream.Position = Header.TrackDataOffset;
				byte[] VtxData = MIO0.Decompress(er.ReadBytes((int)(Header.VertexDataMIO0Length & 0xFFFFFF)));
				VertexData = new VertexDataEntry[Header.VertexDataNrEntries];
				for (int i = 0; i < Header.VertexDataNrEntries; i++)
				{
					VertexData[i] = new VertexDataEntry(VtxData, i * 0xE);
				}
				LevelScript = er.ReadBytes((int)(Header.TrackDataEnd - Header.TrackDataOffset - (Header.VertexDataMIO0Length & 0xFFFFFF)));
				er.BaseStream.Position = Header.TexTableOffset;
				List<TexTableEntry> texes = new List<TexTableEntry>();
				while (true)
				{
					var v = new TexTableEntry(er);
					if (v.GPUTexAddress == 0) break;
					texes.Add(v);
				}
				TexTable = texes.ToArray();
			}
			public TrackHeader Header;
			public byte[] DL;
			public VertexDataEntry[] VertexData;
			public byte[] LevelScript;
			public TexTableEntry[] TexTable;
			public class TrackHeader
			{
				public TrackHeader(EndianBinaryReader er)
				{
					DLOffset = er.ReadUInt32();
					DLEnd = er.ReadUInt32();
					TrackDataOffset = er.ReadUInt32();
					TrackDataEnd = er.ReadUInt32();
					TexTableOffset = er.ReadUInt32();
					TexTableEnd = er.ReadUInt32();
					Unknown1 = er.ReadUInt32();
					VertexDataNrEntries = er.ReadUInt32();
					VertexDataMIO0Length = er.ReadUInt32();
					Unknown3 = er.ReadUInt32();
					Unknown4 = er.ReadUInt32();
					Unknown5 = er.ReadUInt32();
				}
				public UInt32 DLOffset;//mio0
				public UInt32 DLEnd;
				public UInt32 TrackDataOffset;//mio0 with vertex data and the level script after it
				public UInt32 TrackDataEnd;
				public UInt32 TexTableOffset;
				public UInt32 TexTableEnd;
				public UInt32 Unknown1;
				public UInt32 VertexDataNrEntries;
				public UInt32 VertexDataMIO0Length;//Seems segmented! and with 0xFFFFFF
				public UInt32 Unknown3;
				public UInt32 Unknown4;
				public UInt32 Unknown5;
			}
			public class VertexDataEntry
			{
				public VertexDataEntry(byte[] Data, int Offset)
				{
					Position = new Vector3(
						(float)IOUtil.ReadS16BE(Data, Offset) / 24f,
						(float)IOUtil.ReadS16BE(Data, Offset + 2) / 24f,
						(float)IOUtil.ReadS16BE(Data, Offset + 4) / 24f);
					TexCoord = new Vector2(
						(float)IOUtil.ReadS16BE(Data, Offset + 6) / 24f,
						(float)IOUtil.ReadS16BE(Data, Offset + 8) / 24f);
					VertexColor = Color.FromArgb(Data[Offset + 10], Data[Offset + 11], Data[Offset + 12]);
					Unknown1 = Data[Offset + 13];
				}
				public Vector3 Position;
				public Vector2 TexCoord;//?
				public Color VertexColor;
				public Byte Unknown1;//padding?
			}
			public class TexTableEntry
			{
				public TexTableEntry(EndianBinaryReader er)
				{
					GPUTexAddress = er.ReadUInt32();
					CompressedSize = er.ReadUInt32();
					DecompressedSize = er.ReadUInt32();
					Unknown2 = er.ReadUInt32();

					if (GPUTexAddress == 0) return;

					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = TexBaseAddress + (GPUTexAddress & 0xFFFFFF);
					TexData = MIO0.Decompress(er.ReadBytes((int)CompressedSize));
					er.BaseStream.Position = curpos;
				}
				public UInt32 GPUTexAddress;//segmented! and with 0xFFFFFF and add the tex base offset, points to mio0
				public UInt32 CompressedSize;
				public UInt32 DecompressedSize;
				public UInt32 Unknown2;//probably padding

				public byte[] TexData;

				public unsafe Bitmap ToBitmap(bool swap = false, bool alpha = false)
				{
					int width = (int)DecompressedSize / 32 / 2;
					int height = 32;
					if (swap)
					{
						height = width;
						width = 32;
					}
					Bitmap b = new Bitmap(width, height);
					BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					uint* result = (uint*)d.Scan0;
					int stride = d.Stride / 4;
					int offs = 0;
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < width; x++)
						{
							if (!alpha)
							{
								result[y * stride + x] = GFXUtil.ConvertColorFormat(
									IOUtil.ReadU16BE(TexData, offs),
									ColorFormat.RGBA5551,
									ColorFormat.ARGB8888);
							}
							else
							{
								byte c = TexData[offs];
								byte a = TexData[offs + 1];
								result[y * stride + x] = GFXUtil.ToColorFormat(a, c, c, c, ColorFormat.ARGB8888);
							}
							offs += 2;
						}
					}
					b.UnlockBits(d);
					return b;
				}
			}

			public LevelScript.MK64Model GetModel()
			{
				return MarioKart.MK64.LevelScript.GetModel(this);
			}

			/*public OBJ ToOBJ()
			{
				/*OBJ o = new OBJ();
				foreach (var v in VertexData)
				{
					o.Vertices.Add(v.Position);
					o.TexCoords.Add(new Vector2(v.TexCoord.X / 32f * 0.75f, -v.TexCoord.Y / 32f * 0.75f));
				}/
				return MarioKart.MK64.LevelScript.Parse(this);
				//return o;
			}

			public MTL ToMTL()
			{
				MTL m = new MTL();
				int i = 0;
				foreach (var t in TexTable)
				{
					var mm = new MTL.MTLMaterial("M" + i + "M");
					mm.DiffuseColor = Color.White;
					mm.DiffuseMapPath = "Tex/" + i + ".png";
					m.Materials.Add(mm);
					mm = new MTL.MTLMaterial("M" + i + "M_swp");
					mm.DiffuseColor = Color.White;
					mm.DiffuseMapPath = "Tex/" + i + "_swp.png";
					m.Materials.Add(mm);
					mm = new MTL.MTLMaterial("M" + i + "M_alp");
					mm.DiffuseColor = Color.White;
					mm.DiffuseMapPath = "Tex/" + i + "_alp.png";
					m.Materials.Add(mm);
					i += (int)t.DecompressedSize / 2048;
				}
				return m;
			}*/
		}

		public class MK64Identifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Roms;
			}

			public override string GetFileDescription()
			{
				return "Mario Kart 64";
			}

			public override string GetFileFilter()
			{
				return "Mario Kart 64 (*.rom)|*.rom";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 0x40 &&
					File.Data[0x20] == 'M' &&
					File.Data[0x21] == 'A' &&
					File.Data[0x22] == 'R' &&
					File.Data[0x23] == 'I' &&
					File.Data[0x24] == 'O' &&
					File.Data[0x25] == 'K' &&
					File.Data[0x26] == 'A' &&
					File.Data[0x27] == 'R' &&
					File.Data[0x28] == 'T' &&
					File.Data[0x29] == '6' &&
					File.Data[0x2A] == '4' &&

					File.Data[0x3B] == 'N' &&
					File.Data[0x3C] == 'K' &&
					File.Data[0x3D] == 'T' &&
					File.Data[0x3E] == 'E') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
