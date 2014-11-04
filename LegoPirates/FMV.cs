using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using System.Drawing;

namespace LegoPirates
{
	public class FMV : FileFormat<FMV.FMVIdentifier>, IViewable
	{
		private readonly static byte[] QuantizationGenerationTable1 = {
			0x10, 0x10, 0x10, 0x10, 0x11, 0x12, 0x15, 0x18, 0x10, 0x10, 0x10, 0x10,
			0x11, 0x13, 0x16, 0x19, 0x10, 0x10, 0x11, 0x12, 0x14, 0x16, 0x19, 0x1D,
			0x10, 0x10, 0x12, 0x15, 0x18, 0x1B, 0x1F, 0x24, 0x11, 0x11, 0x14, 0x18,
			0x1E, 0x23, 0x29, 0x2F, 0x12, 0x13, 0x16, 0x1B, 0x23, 0x2C, 0x36, 0x41,
			0x15, 0x16, 0x19, 0x1F, 0x29, 0x36, 0x46, 0x58, 0x18, 0x19, 0x1D, 0x24,
			0x2F, 0x41, 0x58, 0x73, 0x11, 0x12, 0x18, 0x2F, 0x63, 0x63, 0x63, 0x63,
			0x12, 0x15, 0x1A, 0x42, 0x63, 0x63, 0x63, 0x63, 0x18, 0x1A, 0x38, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x2F, 0x42, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63
		};

		private readonly static byte[] QuantizationGenerationTable2 = {
			0x10, 0x0B, 0x0A, 0x10, 0x18, 0x28, 0x33, 0x3D, 0x0C, 0x0C, 0x0E, 0x13,
			0x1A, 0x3A, 0x3C, 0x37, 0x0E, 0x0D, 0x10, 0x18, 0x28, 0x39, 0x45, 0x38,
			0x0E, 0x11, 0x16, 0x1D, 0x33, 0x57, 0x50, 0x3E, 0x12, 0x16, 0x25, 0x38,
			0x44, 0x6D, 0x67, 0x4D, 0x18, 0x23, 0x37, 0x40, 0x51, 0x68, 0x71, 0x5C,
			0x31, 0x40, 0x4E, 0x57, 0x67, 0x79, 0x78, 0x65, 0x48, 0x5C, 0x5F, 0x62,
			0x70, 0x64, 0x67, 0x63, 0x11, 0x12, 0x18, 0x2F, 0x63, 0x63, 0x63, 0x63,
			0x12, 0x15, 0x1A, 0x42, 0x63, 0x63, 0x63, 0x63, 0x18, 0x1A, 0x38, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x2F, 0x42, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63,
			0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63
		};

		private readonly static uint[] AnotherQuantizationGenerationTable = {
			0x10000,
			0x16315,
			0x14E7B,
			0x12D06,
			0x10000,
			0xC923,
			0x8A8C,
			0x46A1
		};

		private readonly static byte[] ZigZagTable = {
			0x00, 0x08, 0x01, 0x02, 0x09, 0x10, 0x18, 0x11, 0x0A, 0x03, 0x04, 0x0B,
			0x12, 0x19, 0x20, 0x28, 0x21, 0x1A, 0x13, 0x0C, 0x05, 0x06, 0x0D, 0x14,
			0x1B, 0x22, 0x29, 0x30, 0x38, 0x31, 0x2A, 0x23, 0x1C, 0x15, 0x0E, 0x07,
			0x0F, 0x16, 0x1D, 0x24, 0x2B, 0x32, 0x39, 0x3A, 0x33, 0x2C, 0x25, 0x1E,
			0x17, 0x1F, 0x26, 0x2D, 0x34, 0x3B, 0x3C, 0x35, 0x2E, 0x27, 0x2F, 0x36,
			0x3D, 0x3E, 0x37, 0x3F
		};
		private FMVInfo Info;

		public FMV(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new FMVHeader(er);
				Info = new FMVInfo(this, er);
			}
			catch
			{
				er.Close();
			}
		}

		public System.Windows.Forms.Form GetDialog()
		{
			return new UI.FMVViewer(this);
		}

		public FMVHeader Header;
		public class FMVHeader
		{
			public FMVHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "FMV!") throw new SignatureNotCorrectException(Signature, "FMV!", er.BaseStream.Position - 4);
				Version = er.ReadUInt16();
				HeaderLength = er.ReadUInt16();
				Width = er.ReadUInt16();
				Height = er.ReadUInt16();
				NrBlocks = er.ReadUInt32();
				Unknown1 = er.ReadUInt32();
				FrameRate = er.ReadUInt16();
				Flags = er.ReadUInt32();
				Unknown2 = er.ReadUInt32();
				if ((Flags & 4) == 4)
				{
					Unknown3 = er.ReadUInt16();
					AudioRate = er.ReadUInt32();
					Unknown4 = er.ReadByte();
					Unknown5 = er.ReadByte();
					Unknown6 = er.ReadByte();
					Unknown7 = er.ReadByte();
				}
			}
			public String Signature;
			public UInt16 Version;
			public UInt16 HeaderLength;
			public UInt16 Width;
			public UInt16 Height;
			public UInt32 NrBlocks;
			public UInt32 Unknown1;
			public UInt16 FrameRate;
			public UInt32 Flags;
			public UInt32 Unknown2;
			//If Flags & 4, means audio
			public UInt16 Unknown3;
			public UInt32 AudioRate;
			public Byte Unknown4;
			public Byte Unknown5;
			public Byte Unknown6;
			public Byte Unknown7;
		}

		private class FMVk
		{
			public FMVk(FMVInfo Info, bool Tables)
			{
				Signature = Info.er.ReadString(Encoding.ASCII, 4);
				if (Signature != "FMVk") throw new SignatureNotCorrectException(Signature, "FMVk", Info.er.BaseStream.Position - 4);
				SectionSize = Info.er.ReadUInt32();
				FrameBlockDataSize = Info.er.ReadUInt16();
				FrameBlockData = DecompressRLE(Info.er.ReadBytes(FrameBlockDataSize), 0, FrameBlockDataSize);
				Quality = Info.er.ReadByte();
				int total = 0;
				if (Tables)
				{
					TableInfo1a = Info.er.ReadBytes(16);
					int q = 0;
					foreach (byte b in TableInfo1a) q += b;
					Table1a = Info.er.ReadBytes(q);
					total += q + 16;
					TableInfo2a = Info.er.ReadBytes(16);
					q = 0;
					foreach (byte b in TableInfo2a) q += b;
					Table2a = Info.er.ReadBytes(q);
					total += q + 16;
					TableInfo1b = Info.er.ReadBytes(16);
					q = 0;
					foreach (byte b in TableInfo1b) q += b;
					Table1b = Info.er.ReadBytes(q);
					total += q + 16;
					TableInfo2b = Info.er.ReadBytes(16);
					q = 0;
					foreach (byte b in TableInfo2b) q += b;
					Table2b = Info.er.ReadBytes(q);
					total += q + 16;
					ReadTable(Info, TableInfo1a, Table1a, 0, 0);
					ReadTable(Info, TableInfo2a, Table2a, 1, 0);
					ReadTable(Info, TableInfo1b, Table1b, 0, 1);
					ReadTable(Info, TableInfo2b, Table2b, 1, 1);
				}
				FrameData = Info.er.ReadBytes((int)SectionSize - 3 - FrameBlockDataSize - total);//(Tables ? 0x19C : 0));
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt16 FrameBlockDataSize;
			public byte[] FrameBlockData;
			public byte Quality;

			public byte[] TableInfo1a;
			public byte[] Table1a;
			public byte[] TableInfo2a;
			public byte[] Table2a;
			public byte[] TableInfo1b;
			public byte[] Table1b;
			public byte[] TableInfo2b;
			public byte[] Table2b;

			public byte[] FrameData;

			private void ReadTable(FMVInfo Info, byte[] TableInfo, byte[] Table, int r1, int r2)
			{
				if (r1 == 1)
				{
					CreateTables(ref Info.field_1200[r2], ref Info.field_1600[r2], ref Info.field_1900[r2], ref Info.field_1A04[r2], ref Info.field_1A4C[r2], TableInfo);
					Info.field_1800[r2] = new byte[Table.Length];
					Array.Copy(Table, Info.field_1800[r2], Table.Length);
				}
				else
				{
					CreateTables(ref Info.field_0[r2], ref Info.field_400[r2], ref Info.field_700[r2], ref Info.field_804[r2], ref Info.field_84C[r2], TableInfo);
					Info.field_600[r2] = new byte[Table.Length];
					Array.Copy(Table, Info.field_600[r2], Table.Length);
				}
			}

			private void CreateTables(ref byte[] Table0x0, ref ushort[] Table0x400, ref byte[] Table0x700, ref uint[] Table0x804, ref uint[] Table0x84C, byte[] TableInfo)
			{
				List<byte> data = new List<byte>();
				for (int i = 0; i < 16; i++)
				{
					for (int j = 0; j < TableInfo[i]; j++)
					{
						data.Add((byte)((i + 1) & 0xFF));
					}
				}
				data.Add(0);
				Table0x700 = data.ToArray();
				Table0x804 = new uint[18];
				Table0x84C = new uint[18];
				List<ushort> table0x400 = new List<ushort>();
				uint v7 = 0;
				int v8 = 0;
				int v9 = 1;
				do
				{
					for (Table0x84C[v9] = (uint)(v8 - v7); v9 == Table0x700[v8]; table0x400.Add((ushort)v7++), v8++) ;
					uint v10 = v7 << (16 - v9);
					Table0x804[v9++] = v10;
					v7 *= 2;
				}
				while (v9 <= 16);
				Table0x804[v9] = 0xFFFFFFFF;
				Table0x400 = table0x400.ToArray();
				Table0x0 = new byte[0x400];
				//This just fills with 0xFF
				//ushort[] _data = new ushort[0x200];
				//sub_213474C(ref _data, 0xFF, 0x400);
				for (int i = 0; i < 0x400; i++)
				{
					Table0x0[i] = 0xFF;
				}
				for (int i = 0; i < v8; i++)
				{
					int v13 = Table0x700[i];
					if (v13 <= 10)
					{
						int v14 = 10 - v13;
						int v15 = 1 << v14;
						int v16 = 0;
						int v17 = Table0x400[i];
						if (1 << v14 > 0)
						{
							do
							{
								Table0x0[(v17 << v14) + v16++] = (byte)i;
							}
							while (v16 < v15);
						}
					}
				}
			}
		}

		private class FMVd
		{
			public FMVd(FMVInfo Info)
			{
				Signature = Info.er.ReadString(Encoding.ASCII, 4);
				if (Signature != "FMVd") throw new SignatureNotCorrectException(Signature, "FMVd", Info.er.BaseStream.Position - 4);
				SectionSize = Info.er.ReadUInt32();
				FrameBlockDataSize = Info.er.ReadUInt16();
				FrameBlockData = DecompressRLE(Info.er.ReadBytes(FrameBlockDataSize), 0, FrameBlockDataSize);
				Marker = Info.er.ReadByte();
				FrameData = Info.er.ReadBytes((int)SectionSize - 3 - FrameBlockDataSize);
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt16 FrameBlockDataSize;
			public byte[] FrameBlockData;
			public byte Marker;//?
			public byte[] FrameData;
		}

		public Bitmap GetNextFrame(out byte[] Audio)
		{
			if (Info.er != null && Info.er.BaseStream.Position >= Info.er.BaseStream.Length)
			{
				Info.er.Close();
				Info.er = null;
				Audio = null;
				return null;
			}
			Audio = null;
			String s = Info.er.ReadString(Encoding.ASCII, 4);
			Info.er.BaseStream.Position -= 4;
			switch (s)
			{
				case "FMVk":
					{
						var v = new FMVk(Info, Info.FirstKeyFrame);
						Info.FirstKeyFrame = false;
						GenerateQuantizationTables(Info, v.Quality);
						Info.LastFrame = ToBitmap(Info, v.FrameBlockData, v.FrameData);
						return Info.LastFrame;
					}
				case "FMVd":
					{
						var v = new FMVd(Info);
						GenerateQuantizationTables(Info, v.Marker);
						Info.LastFrame = ToBitmap(Info, v.FrameBlockData, v.FrameData);
						return Info.LastFrame;
					}
				case "FMVn":
					Info.er.BaseStream.Position += 8;
					return Info.LastFrame;
				case "FMA\0":
					Info.er.BaseStream.Position += 4;
					int length = (int)Info.er.ReadUInt32();
					Audio = Info.er.ReadBytes(length);
					return null;
				default:
					return null;
			}
		}

		public void Close()
		{
			if (Info.er != null)
			{
				Info.er.Close();
				Info.er = null;
			}
		}

		private static void GenerateQuantizationTables(FMVInfo Info, uint Quality)
		{
			uint r0;
			uint r1 = Quality;
			uint r2;
			uint r3;
			uint r4;
			uint r6;
			uint r7;
			uint r8;
			uint r11;
			uint r12;
			uint lr;

			byte[] QuantizationGenerationTable = QuantizationGenerationTable1;//If 16 bit at offset 4 in header == 0x113

			Info.QuantizationTables[0] = new uint[64];
			Info.QuantizationTables[1] = new uint[64];

			if (r1 <= 0) r1 = 1;
			if (r1 > 100) r1 = 100;
			if (r1 >= 50)
			{
				r0 = r1 << 1;
				r0 = 0xC8 - r0;
			}
			else
			{
				r0 = 0x1388 / r1;
				//r0 = 0; 
			}
			r7 = 0;
			while (true)
			{
				r6 = 0;
				r1 = 0x51EB851F;
				r11 = 1;
				while (true)
				{
					r2 = QuantizationGenerationTable[64 * r7 + r6];
					r4 = (uint)((int)r6 >> 3);
					r12 = r6 & 7;
					r2 = r0 * r2;
					r3 = r2 + 0x32;

					ulong tmpl = (ulong)(((long)(int)r1) * ((long)(int)r3));
					r2 = (uint)(tmpl & 0xFFFFFFFF);
					r8 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

					r2 = r3 >> 31;
					r3 = AnotherQuantizationGenerationTable[r12];
					lr = AnotherQuantizationGenerationTable[r4];
					r8 = r2 + (uint)((int)r8 >> 5);

					tmpl = (ulong)(((long)(int)lr) * ((long)(int)r3));
					r12 = (uint)(tmpl & 0xFFFFFFFF);
					r3 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

					if (r8 <= 0) r8 = r11;
					r12 >>= 16;
					if (r8 > 255) r8 = 255;
					r12 |= r3 << 16;

					tmpl = (ulong)(((long)(int)r8) * ((long)(int)r12));
					r3 = (uint)(tmpl & 0xFFFFFFFF);
					r12 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

					r12 = r6 << 29;
					r6++;
					r4 += r12 >> 26;
					Info.QuantizationTables[r7][r4] = r3;
					if (r6 >= 0x40) break;
				}
				r7++;
				if (r7 >= 2) break;
			}
		}

		private static Bitmap ToBitmap(FMVInfo Info, byte[] FrameBlockData, byte[] FrameData)
		{
			Info.BitReg = 0;
			Info.BitRegNrBits = 0;
			Info.PhaseInfos[0].Field10 = 0;
			Info.PhaseInfos[1].Field10 = 0;
			Info.PhaseInfos[2].Field10 = 0;

			int offset = 0;
			for (int y = 0; y < Info.NrTilesY; y++)
			{
				for (int x = 0; x < Info.NrTilesX; x++)
				{
					int type = FrameBlockData[y * Info.NrTilesX + x];
					switch (type)
					{
						case 0:
							{
							}
							break;
						case 1:
							{
								if (Info.BitRegNrBits < 5) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
								Info.BitRegNrBits -= 5;
								int a = (int)(Info.BitReg >> Info.BitRegNrBits) & 0x1F;
								if (a > 15) a -= 32;
								if (Info.BitRegNrBits < 5) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
								Info.BitRegNrBits -= 5;
								int b = (int)(Info.BitReg >> Info.BitRegNrBits) & 0x1F;
								if (b > 15) b -= 32;
								for (int _y = 0; _y < 16; _y++)
								{
									for (int _x = 0; _x < 16; _x++)
									{
										Info.PhaseInfos[0].PhaseData[((y * 16) + _y) * Info.PhaseInfos[0].PhaseDataWidth + ((x * 16) + _x)] = Info.PhaseInfos[0].PhaseDataOld[(((y * 16) + _y) - b) * Info.PhaseInfos[0].PhaseDataWidth + ((x * 16) + _x) - a];
									}
								}
								for (int _y = 0; _y < 8; _y++)
								{
									for (int _x = 0; _x < 8; _x++)
									{
										Info.PhaseInfos[1].PhaseData[((y * 8) + _y) * Info.PhaseInfos[1].PhaseDataWidth + ((x * 8) + _x)] = Info.PhaseInfos[1].PhaseDataOld[(((y * 8) + _y) - b / 2) * Info.PhaseInfos[1].PhaseDataWidth + ((x * 8) + _x) - a / 2];
									}
								}
								for (int _y = 0; _y < 8; _y++)
								{
									for (int _x = 0; _x < 8; _x++)
									{
										Info.PhaseInfos[2].PhaseData[((y * 8) + _y) * Info.PhaseInfos[2].PhaseDataWidth + ((x * 8) + _x)] = Info.PhaseInfos[2].PhaseDataOld[(((y * 8) + _y) - b / 2) * Info.PhaseInfos[2].PhaseDataWidth + ((x * 8) + _x) - a / 2];
									}
								}
							}
							break;
						case 2:
							{
								if (Info.BitRegNrBits < 8) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
								Info.BitRegNrBits -= 8;
								int a = (int)(Info.BitReg >> Info.BitRegNrBits) & 0xFF;
								if (Info.BitRegNrBits < 8) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
								Info.BitRegNrBits -= 8;
								int b = (int)(Info.BitReg >> Info.BitRegNrBits) & 0xFF;
								if (Info.BitRegNrBits < 8) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
								Info.BitRegNrBits -= 8;
								int c = (int)(Info.BitReg >> Info.BitRegNrBits) & 0xFF;
								for (int _y = 0; _y < 16; _y++)
								{
									for (int _x = 0; _x < 16; _x++)
									{
										Info.PhaseInfos[0].PhaseData[((y * 16) + _y) * Info.PhaseInfos[0].PhaseDataWidth + ((x * 16) + _x)] = (byte)a;
									}
								}
								for (int _y = 0; _y < 8; _y++)
								{
									for (int _x = 0; _x < 8; _x++)
									{
										Info.PhaseInfos[1].PhaseData[((y * 8) + _y) * Info.PhaseInfos[1].PhaseDataWidth + ((x * 8) + _x)] = (byte)b;
									}
								}
								for (int _y = 0; _y < 8; _y++)
								{
									for (int _x = 0; _x < 8; _x++)
									{
										Info.PhaseInfos[2].PhaseData[((y * 8) + _y) * Info.PhaseInfos[2].PhaseDataWidth + ((x * 8) + _x)] = (byte)c;
									}
								}
							}
							break;
						case 255:
							{
								for (int i = 0; i < Info.NrPhases; i++)
								{
									for (int j = 0; j < Info.PhaseInfos[i].NrBlocksX * Info.PhaseInfos[i].NrBlocksY; j++)
									{
										ushort[] r7 = new ushort[0x40];
										if (Info.BitRegNrBits < 16) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
										int a = (int)(Info.BitReg >> (Info.BitRegNrBits - 10)) & 0x3FF;
										int q = Info.field_0[Info.PhaseInfos[i].Table1Idx][a];
										int r10;
										if (q >= 255)
										{
											uint v12;
											if (Info.BitRegNrBits >= 16) v12 = Info.BitReg >> (Info.BitRegNrBits - 16);
											else v12 = Info.BitReg << (16 - Info.BitRegNrBits);
											v12 &= 0xFFFF;
											int qq = 0xB;
											while (true)
											{
												if (v12 >= Info.field_804[Info.PhaseInfos[i].Table1Idx][qq]) qq++;
												else break;
											}
											int v16 = (int)(((1 << qq) - 1) & (Info.BitReg >> (Info.BitRegNrBits - qq))) + (int)Info.field_84C[Info.PhaseInfos[i].Table1Idx][qq];//*(v15 + 531);
											Info.BitRegNrBits -= qq;
											r10 = Info.field_600[Info.PhaseInfos[i].Table1Idx][v16];
										}
										else
										{
											int q2 = Info.field_700[Info.PhaseInfos[i].Table1Idx][q];
											Info.BitRegNrBits -= q2;
											r10 = Info.field_600[Info.PhaseInfos[i].Table1Idx][q];
										}
										uint _r5 = 0;
										sub_213474C(ref r7, 0, 0x80);
										if (r10 != 0)
										{
											if (Info.BitRegNrBits < r10) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
											Info.BitRegNrBits -= r10;
											int mask = (1 << r10) - 1;
											_r5 = (uint)((int)(Info.BitReg >> Info.BitRegNrBits) & mask);
											if (_r5 < (1 << (r10 - 1)))
											{
												uint _r0 = 0xFFFFFFFF;
												_r0 = _r5 + (_r0 << r10);
												_r5 = (uint)(((int)_r0) + 1);
											}
										}
										Info.PhaseInfos[i].Field10 += (int)_r5;
										r7[0] = (ushort)Info.PhaseInfos[i].Field10;

										int counter = 1;
										while (true)
										{
											if (Info.BitRegNrBits < 16) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
											int c = (int)(Info.BitReg >> (Info.BitRegNrBits - 10)) & 0x3FF;
											int qc = Info.field_1200[Info.PhaseInfos[i].Table2Idx][c];
											int r1;
											if (qc >= 255)
											{
												uint r0;
												if (Info.BitRegNrBits < 16)
												{
													r0 = 16 - (uint)Info.BitRegNrBits;
													r0 = (uint)(Info.BitReg << (int)r0);
												}
												else
												{
													r0 = (uint)Info.BitRegNrBits - 16;
													r0 = (uint)(Info.BitReg >> (int)r0);
												}
												r0 <<= 16;
												uint r9 = r0 >> 16;
												int r3 = 0xB;
												while (true)
												{
													if (r9 >= Info.field_1A04[Info.PhaseInfos[i].Table2Idx][r3]) r3++;
													else break;
												}
												uint r5 = (uint)1 << r3;
												r9 = r5 - 1;
												r5 = (uint)(Info.BitRegNrBits - r3);
												r5 = r9 & (Info.BitReg >> (int)r5);
												int r2 = (int)Info.field_1A4C[Info.PhaseInfos[i].Table2Idx][r3];
												Info.BitRegNrBits -= r3;
												r1 = Info.field_1800[Info.PhaseInfos[i].Table2Idx][r5 + r2];
											}
											else
											{
												int q2c = Info.field_1900[Info.PhaseInfos[i].Table2Idx][qc];
												Info.BitRegNrBits -= q2c;
												r1 = Info.field_1800[Info.PhaseInfos[i].Table2Idx][qc];
											}
											r10 = r1 & 0xF;
											if (r10 != 0)
											{
												counter += r1 >> 4;
												if (Info.BitRegNrBits < r10) FillBits(FrameData, ref Info.BitReg, ref Info.BitRegNrBits, ref offset);
												Info.BitRegNrBits -= r10;
												int mask = (1 << r10) - 1;
												uint d = (uint)((Info.BitReg >> Info.BitRegNrBits) & mask);
												uint r0;
												if (d < (1 << (r10 - 1)))
												{
													r0 = 0xFFFFFFFF;
													r0 = (uint)(d + (r0 << r10));
													d = r0 + 1;
												}
												r0 = ZigZagTable[counter];
												counter++;
												r7[r0] = (ushort)d;
											}
											else if (r1 != 0xF0) break;
											else counter += 16;
											if (counter >= 0x40) goto con;
											continue;
										}
									con:
										byte[] data = sub_20A66B0(r7, Info.QuantizationTables[Info.PhaseInfos[i].QTIdx]);
										if (i == 0)
										{
											for (int qq = 0; qq < 64; qq++)
											{
												int _x = (qq % 8) + x * 16 + (j % 2 * 8);
												int _y = (qq / 8) + y * 16 + (j / 2 * 8);
												Info.PhaseInfos[i].PhaseData[_y * Info.PhaseInfos[i].PhaseDataWidth + _x] = data[qq];
											}
										}
										else if (i == 1)
										{
											for (int qq = 0; qq < 64; qq++)
											{
												int _x = (qq % 8) + x * 8;
												int _y = (qq / 8) + y * 8;
												Info.PhaseInfos[i].PhaseData[_y * Info.PhaseInfos[i].PhaseDataWidth + _x] = data[qq];
											}
										}
										else
										{
											for (int qq = 0; qq < 64; qq++)
											{
												int _x = (qq % 8) + x * 8;
												int _y = (qq / 8) + y * 8;
												Info.PhaseInfos[i].PhaseData[_y * Info.PhaseInfos[i].PhaseDataWidth + _x] = data[qq];
											}
										}
										continue;
									}
								}
							}
							break;
					}
				}
			}
			Bitmap bb4 = new Bitmap(Info.Width, Info.Height);
			for (int y = 0; y < Info.Height; y++)
			{
				for (int x = 0; x < Info.Width; x++)
				{
					float Y = Info.PhaseInfos[0].PhaseData[y * Info.PhaseInfos[0].PhaseDataWidth + x] / 256f;
					float U = Info.PhaseInfos[1].PhaseData[y / 2 * Info.PhaseInfos[1].PhaseDataWidth + x / 2] / 256f - 0.5f;
					float V = Info.PhaseInfos[2].PhaseData[y / 2 * Info.PhaseInfos[2].PhaseDataWidth + x / 2] / 256f - 0.5f;
					float R = Y + V * 1.13983f;
					float G = Y - U * 0.39465f - V * 0.58060f;
					float B = Y + U * 2.03211f;
					if (R < 0) R = 0;
					if (R > 1) R = 1;
					if (G < 0) G = 0;
					if (G > 1) G = 1;
					if (B < 0) B = 0;
					if (B > 1) B = 1;
					bb4.SetPixel(x, y, Color.FromArgb((int)(R * 255), (int)(G * 255), (int)(B * 255)));
				}
			}
			Array.Copy(Info.PhaseInfos[0].PhaseData, Info.PhaseInfos[0].PhaseDataOld, Info.PhaseInfos[0].PhaseData.Length);
			Array.Copy(Info.PhaseInfos[1].PhaseData, Info.PhaseInfos[1].PhaseDataOld, Info.PhaseInfos[1].PhaseData.Length);
			Array.Copy(Info.PhaseInfos[2].PhaseData, Info.PhaseInfos[2].PhaseDataOld, Info.PhaseInfos[2].PhaseData.Length);
			return bb4;
		}

		private static void FillBits(byte[] FrameData, ref uint bitreg, ref int nrbits, ref int offset)
		{
			while (nrbits <= 24)
			{
				byte a;
				if (offset + 1 < FrameData.Length) a = FrameData[offset++];
				else a = 0;
				bitreg = bitreg << 8 | a;
				nrbits += 8;
			}
		}

		private static void sub_213474C(ref ushort[] r0, uint r1, uint r2)
		{
			byte[] data = new byte[r0.Length * 2];
			for (int i = 0; i < r0.Length; i++)
			{
				data[i * 2] = (byte)(r0[i] & 0xFF);
				data[i * 2 + 1] = (byte)((r0[i] >> 8) & 0xFF);
			}
			int Offset = 0;

			uint r3 = r1 & 0xFF;
			if (r2 >= 0x20)
			{
				if (r3 != 0)
				{
					r1 = r3 << 16;
					r1 |= r3 << 24;
					r1 |= r3 << 8;
					r3 |= r1;
				}
				r1 = r2 >> 5;
				while (r1 != 0)
				{
					//0
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					//4
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					//8
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					//12
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					//16
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					//20
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					//24
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					//28
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					r1--;
				}
				r1 = r2 & 0x1F;
				r1 >>= 2;
				while (r1 != 0)
				{
					data[Offset++] = (byte)(r3 & 0xFF);
					data[Offset++] = (byte)((r3 >> 8) & 0xFF);
					data[Offset++] = (byte)((r3 >> 16) & 0xFF);
					data[Offset++] = (byte)((r3 >> 24) & 0xFF);
					r1--;
				}
				r2 &= 3;
			}
			if (r2 == 0) goto ret;
			r1 = r3 & 0xFF;
			while (true)
			{
				data[Offset++] = (byte)r1;
				r2--;
				if (r2 == 0) break;
			}
		ret:
			for (int i = 0; i < r0.Length; i++)
			{
				r0[i] = (ushort)(data[i * 2] | (data[i * 2 + 1] << 8));
			}
		}

		private static byte[] sub_20A66B0(ushort[] indata, uint[] QuantizationTable)
		{
			byte[] data = new byte[indata.Length * 2];
			for (int i = 0; i < indata.Length; i++)
			{
				data[i * 2] = (byte)(indata[i] & 0xFF);
				data[i * 2 + 1] = (byte)((indata[i] >> 8) & 0xFF);
			}
			int DOffset = 0;
			int QTOffset = 0;
			int R0Offset = 0;
			int R5Offset = 0;

			int Counter = 0;
			byte[] r0data = new byte[64];
			uint[] r5data = new uint[64];

			uint r2;
			uint r3;
			uint r4;
			uint r5;
			uint r6;
			uint r7;
			uint r8;
			uint r9;
			uint r10;
			uint r11;
			uint r12;
			uint lr;

			uint SP_660_var_644;
			uint SP_660_var_648;
			uint SP_660_var_64C;
			uint SP_660_var_650;
			uint SP_660_var_654;
			uint SP_660_var_658;

			while (true)
			{
				bool tmp;
				r8 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 2);
				if (tmp = r8 == 0)
				{
					r4 = IOUtil.ReadU32LE(data, DOffset + 4);
					if (tmp = r4 == 0)
					{
						r4 = IOUtil.ReadU32LE(data, DOffset + 8);
						if (tmp = r4 == 0)
						{
							r4 = IOUtil.ReadU32LE(data, DOffset + 12);
							tmp = r4 == 0;
						}
					}
				}
				if (tmp)
				{
					r6 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 0);
					r4 = QuantizationTable[QTOffset + 0];
					r4 = r6 * r4;
					r5data[R5Offset + 0] = r4;
					r5data[R5Offset + 8] = r4;
					r5data[R5Offset + 16] = r4;
					r5data[R5Offset + 24] = r4;
					r5data[R5Offset + 32] = r4;
					r5data[R5Offset + 40] = r4;
					r5data[R5Offset + 48] = r4;
					r5data[R5Offset + 56] = r4;
					goto loc_20A687C;
				}
				r7 = QuantizationTable[QTOffset + 1];
				r6 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 6);
				lr = r8 * r7;
				r4 = QuantizationTable[QTOffset + 3];
				r8 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 10);
				r7 = r6 * r4;
				r6 = QuantizationTable[QTOffset + 5];
				r4 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 0);
				r9 = QuantizationTable[QTOffset + 0];
				r6 = r8 * r6;
				r12 = r4 * r9;
				r8 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 4);
				r4 = QuantizationTable[QTOffset + 2];
				r10 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 8);
				r4 = r8 * r4;
				r9 = QuantizationTable[QTOffset + 4];
				r8 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 12);
				r11 = QuantizationTable[QTOffset + 6];
				r9 = r10 * r9;
				r11 = r8 * r11;
				r10 = r12 + r9;
				r9 = r12 - r9;
				r8 = r4 + r11;
				r12 = r4 - r11;
				r4 = 0x16A0A;

				ulong tmpl = (ulong)(((long)(int)r12) * ((long)(int)r4));
				r11 = (uint)(tmpl & 0xFFFFFFFF);
				r4 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r11 >>= 16;
				r11 |= r4 << 16;
				r4 = r11 - r8;
				r11 = r10 + r8;
				r8 = r10 - r8;
				r10 = r9 + r4;
				r4 = r9 - r4;
				SP_660_var_658 = r4;
				SP_660_var_648 = r10;
				r10 = r6 + r7;
				r9 = (uint)(int)IOUtil.ReadS16LE(data, DOffset + 14);
				r4 = QuantizationTable[QTOffset + 7];
				r6 = r6 - r7;
				r4 = r9 * r4;
				r7 = lr + r4;
				SP_660_var_64C = r11;
				r9 = lr - r4;
				r4 = r7 + r10;
				r11 = r7 - r10;
				r7 = 0x16A0A;

				tmpl = (ulong)(((long)(int)r11) * ((long)(int)r7));
				r10 = (uint)(tmpl & 0xFFFFFFFF);
				r7 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r11 = r10 >> 16;
				r11 |= r7 << 16;
				r10 = 0x1D907;
				r7 = r6 + r9;

				tmpl = (ulong)(((long)(int)r7) * ((long)(int)r10));
				r12 = (uint)(tmpl & 0xFFFFFFFF);
				r10 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r7 = r12 >> 16;
				SP_660_var_654 = r10;
				r7 |= r10 << 16;
				r10 = 0x11518;

				tmpl = (ulong)(((long)(int)r10) * ((long)(int)r9));
				r12 = (uint)(tmpl & 0xFFFFFFFF);
				r9 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r10 = r12 >> 16;
				r10 |= r9 << 16;
				r9 = 0xFFFD630B;
				r10 = r10 - r7;

				tmpl = (ulong)(((long)(int)r9) * ((long)(int)r6));
				r12 = (uint)(tmpl & 0xFFFFFFFF);
				r6 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r9 = r12 >> 16;
				r9 |= r6 << 16;
				r6 = r9 + r7;
				r9 = r6 - r4;
				r7 = r11 - r9;
				r6 = r10 + r7;
				r10 = SP_660_var_64C;
				r11 = r10 + r4;
				r4 = r10 - r4;
				r5data[R5Offset + 0] = r11;
				r5data[R5Offset + 56] = r4;
				r4 = SP_660_var_648;
				r10 = r4 + r9;
				r4 = r4 - r9;
				r5data[R5Offset + 8] = r10;
				r5data[R5Offset + 48] = r4;
				r4 = SP_660_var_658;
				r9 = r4 + r7;
				r4 = r4 - r7;
				r5data[R5Offset + 16] = r9;
				r5data[R5Offset + 40] = r4;
				r7 = r8 + r6;
				r4 = r8 - r6;
				r5data[R5Offset + 32] = r7;
				r5data[R5Offset + 24] = r4;
			loc_20A687C:
				DOffset += 16;
				Counter++;
				QTOffset += 8;
				R5Offset++;
				if (Counter >= 8) break;
			}
			Counter = 0;
			R5Offset = 0;
			r4 = 0xFF;
			while (true)
			{
				r5 = r5data[R5Offset + 4];
				r2 = r5data[R5Offset + 0];
				r8 = r5data[R5Offset + 6];
				r7 = r5data[R5Offset + 2];
				r3 = r2 + r5;
				r6 = r2 - r5;
				r2 = r7 + r8;
				r9 = r7 - r8;
				r7 = 0x16A0A;
				r12 = r3 + r2;

				ulong tmpl = (ulong)(((long)(int)r9) * ((long)(int)r7));
				r8 = (uint)(tmpl & 0xFFFFFFFF);
				r7 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r8 >>= 16;
				r8 |= r7 << 16;
				r7 = r8 - r2;
				r3 = r3 - r2;
				r5 = r5data[R5Offset + 3];
				r8 = r5data[R5Offset + 5];
				r2 = r6 + r7;
				r11 = r6 - r7;
				r6 = r8 + r5;
				r9 = r8 - r5;
				r5 = 0xFFFD630B;
				SP_660_var_644 = r6;

				tmpl = (ulong)(((long)(int)r5) * ((long)(int)r9));
				r10 = (uint)(tmpl & 0xFFFFFFFF);
				r8 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r5 = r10 >> 16;
				r7 = r5data[R5Offset + 7];
				r6 = r5data[R5Offset + 1];
				r5 |= r8 << 16;
				r8 = r6 + r7;
				r7 = r6 - r7;
				r6 = 0x1D907;
				r10 = r9 + r7;

				tmpl = (ulong)(((long)(int)r10) * ((long)(int)r6));
				r9 = (uint)(tmpl & 0xFFFFFFFF);
				r6 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				SP_660_var_650 = r6;
				r6 = r9 >> 16;
				r9 = SP_660_var_650;
				R5Offset += 8;
				r6 |= r9 << 16;
				r9 = 0x11518;
				r5 = r5 + r6;

				tmpl = (ulong)(((long)(int)r9) * ((long)(int)r7));
				r10 = (uint)(tmpl & 0xFFFFFFFF);
				r7 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r9 = r10 >> 16;
				r9 |= r7 << 16;
				r9 = r9 - r6;
				r6 = SP_660_var_644;
				r7 = r8 + r6;
				r6 = r8 - r6;
				r8 = 0x16A0A;
				r5 = r5 - r7;

				tmpl = (ulong)(((long)(int)r6) * ((long)(int)r8));
				r10 = (uint)(tmpl & 0xFFFFFFFF);
				r8 = (uint)((tmpl >> 32) & 0xFFFFFFFF);

				r6 = r10 >> 16;
				r6 |= r8 << 16;
				r8 = r6 - r5;
				r6 = r9 + r8;
				r9 = r12 + r7;

				r9 = (uint)((int)r9 >> 19);
				r9 += 0x80;
				r7 = r12 - r7;
				if (((int)r9) < 0) r9 = 0;
				if (r9 > 255) r9 = r4;

				r7 = (uint)((int)r7 >> 19);
				r7 += 0x80;
				if (((int)r7) < 0) r7 = 0;
				r0data[R0Offset + 0] = (byte)r9;
				if (r7 > 255) r7 = r4;

				r0data[R0Offset + 7] = (byte)r7;
				r7 = r2 + r5;

				r7 = (uint)((int)r7 >> 19);
				r7 += 0x80;
				r2 = r2 - r5;
				if (((int)r7) < 0) r7 = 0;
				if (r7 > 255) r7 = r4;

				r2 = (uint)((int)r2 >> 19);
				r2 += 0x80;
				if (((int)r2) < 0) r2 = 0;
				r0data[R0Offset + 1] = (byte)r7;
				if (r2 > 255) r2 = r4;

				r0data[R0Offset + 6] = (byte)r2;
				r2 = r11 + r8;

				r2 = (uint)((int)r2 >> 19);
				r5 = r2 + 0x80;
				r2 = r11 - r8;
				if (((int)r5) < 0) r5 = 0;
				if (r5 > 255) r5 = r4;

				r2 = (uint)((int)r2 >> 19);
				r2 += 0x80;
				if (((int)r2) < 0) r2 = 0;
				r0data[R0Offset + 2] = (byte)r5;
				if (r2 > 255) r2 = r4;

				r0data[R0Offset + 5] = (byte)r2;
				r2 = r3 + r6;

				r2 = (uint)((int)r2 >> 19);
				r5 = r2 + 0x80;
				r2 = r3 - r6;
				if (((int)r5) < 0) r5 = 0;
				if (r5 > 255) r5 = r4;

				r2 = (uint)((int)r2 >> 19);
				r2 += 0x80;
				if (((int)r2) < 0) r2 = 0;
				r0data[R0Offset + 4] = (byte)r5;
				if (r2 > 255) r2 = r4;

				r0data[R0Offset + 3] = (byte)r2;
				R0Offset += 8;
				Counter++;
				if (Counter >= 8) break;
			}
			return r0data;
		}

		private static byte[] DecompressRLE(byte[] Data, int DataOffset, int Length)
		{
			List<byte> OutputData = new List<byte>();
			int Offset = DataOffset;
			int R5 = Data[Offset++];
			int R12;
			int LR;
			while (true)
			{
				R12 = Data[Offset++];
				if (R12 == R5)
				{
					LR = Data[Offset++];
					if (LR <= 2)
					{
						R12 = 0;
						while (true)
						{
							R12++;
							OutputData.Add((byte)R5);
							if (R12 > LR) break;
						}
						goto loc_20A5AA0;
					}
					if ((LR & 0x80) != 0)
					{
						R12 = Data[Offset++];
						LR <<= 25;
						LR = R12 + (LR >> 17);
					}
					R12 = Data[Offset++];
					int R8 = 0;
					while (true)
					{
						R8++;
						OutputData.Add((byte)R12);
						if (R8 > LR) break;
					}
					goto loc_20A5AA0;
				}
				OutputData.Add((byte)R12);
			loc_20A5AA0:
				if (Offset >= (Length + DataOffset)) break;
			}
			return OutputData.ToArray();
		}

		private class FMVInfo
		{
			public FMVInfo(FMV f, EndianBinaryReader er)
			{
				this.er = er;
				Width = f.Header.Width;
				Height = f.Header.Height;
				NrTilesX = Width / 16;
				NrTilesY = Height / 16;
				PhaseInfos[0] = new PhaseInfo(this, 2, 2, 0, 0, 0);
				PhaseInfos[1] = new PhaseInfo(this, 1, 1, 1, 1, 1);
				PhaseInfos[2] = new PhaseInfo(this, 1, 1, 1, 1, 1);
			}
			public EndianBinaryReader er;

			public byte[][] field_0 = new byte[2][];
			public ushort[][] field_400 = new ushort[2][];
			public byte[][] field_600 = new byte[2][];
			public byte[][] field_700 = new byte[2][];
			public uint[][] field_804 = new uint[2][];
			public uint[][] field_84C = new uint[2][];

			public byte[][] field_1200 = new byte[2][];
			public ushort[][] field_1600 = new ushort[2][];
			public byte[][] field_1800 = new byte[2][];
			public byte[][] field_1900 = new byte[2][];
			public uint[][] field_1A04 = new uint[2][];
			public uint[][] field_1A4C = new uint[2][];

			public bool FirstKeyFrame = true;

			public int Width;
			public int Height;
			public int NrPhases = 3;
			public int Quality = 0;
			public uint[][] QuantizationTables = new uint[2][];
			public int NrTilesX;
			public int NrTilesY;
			public uint BitReg = 0;
			public int BitRegNrBits = 0;
			public PhaseInfo[] PhaseInfos = new PhaseInfo[3];

			public Bitmap LastFrame;

			public class PhaseInfo
			{
				public PhaseInfo(FMVInfo Info, int NrBlocksX, int NrBlocksY, int QTIdx, int Table1Idx, int Table2Idx)
				{
					this.NrBlocksX = NrBlocksX;
					this.NrBlocksY = NrBlocksY;
					this.QTIdx = QTIdx;
					this.Table1Idx = Table1Idx;
					this.Table2Idx = Table2Idx;
					Field10 = 0;
					PhaseDataWidth = 8 * Info.NrTilesX * NrBlocksX;
					PhaseDataHeight = 8 * Info.NrTilesY * NrBlocksY;
					PhaseData = new byte[PhaseDataWidth * PhaseDataHeight];
					PhaseDataOld = new byte[PhaseDataWidth * PhaseDataHeight];
				}
				public int NrBlocksX;
				public int NrBlocksY;
				public int QTIdx;
				public int Table1Idx;
				public int Table2Idx;
				public int Field10;
				public int PhaseDataWidth;
				public int PhaseDataHeight;
				public byte[] PhaseData;
				public byte[] PhaseDataOld;
			}

		}

		public class FMVIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Videos;
			}

			public override string GetFileDescription()
			{
				return "Lego Pirates of the Carribean Cutscenes (FMV)";
			}

			public override string GetFileFilter()
			{
				return "Lego Pirates of the Carribean Cutscenes (*.fmv)|*.fmv";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'M' && File.Data[2] == 'V' && File.Data[3] == '!') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
