using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.GameData;
using System.Windows.Forms;
using LibEveryFileExplorer.Collections;
using Tao.OpenGl;
using MarioKart.UI;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.Math;
using System.ComponentModel;
using LibEveryFileExplorer.ComponentModel;
using LibEveryFileExplorer;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace MarioKart.MKDS.NKM
{
	public class NKMD : FileFormat<NKMD.NKMDIdentifier>, IViewable, IWriteable
	{
		public NKMD()
		{
			Header = new NKMDHeader();
		}

		public NKMD(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new NKMDHeader(er);
				foreach (var v in Header.SectionOffsets)
				{
					er.BaseStream.Position = Header.HeaderSize + v;
					String sig = er.ReadString(Encoding.ASCII, 4);
					er.BaseStream.Position -= 4;
					switch (sig)
					{
						case "OBJI": ObjectInformation = new OBJI(er); break;
						case "PATH": Path = new PATH(er); break;
						case "POIT": Point = new POIT(er); break;
						case "STAG": Stage = new STAG(er); break;
						case "KTPS": KartPointStart = new KTPS(er); break;
						case "KTPJ": KartPointJugem = new KTPJ(er, Header.Version); break;
						case "KTP2": KartPointSecond = new KTP2(er); break;
						case "KTPC": KartPointCannon = new KTPC(er); break;
						case "KTPM": KartPointMission = new KTPM(er); break;
						case "CPOI": CheckPoint = new CPOI(er); break;
						case "CPAT": CheckPointPath = new CPAT(er); break;
						case "IPOI": ItemPoint = new IPOI(er, Header.Version); break;
						case "IPAT": ItemPath = new IPAT(er); break;
						case "EPOI": EnemyPoint = new EPOI(er); break;
						case "EPAT": EnemyPath = new EPAT(er); break;
						case "MEPO": MiniGameEnemyPoint = new MEPO(er); break;
						case "MEPA": MiniGameEnemyPath = new MEPA(er); break;
						case "AREA": Area = new AREA(er); break;
						case "CAME": Camera = new CAME(er); break;
						default:
							throw new Exception("Unknown Section: " + sig);
					}
				}
				if (KartPointCannon == null) KartPointCannon = new KTPC();
				if (KartPointMission == null) KartPointMission = new KTPM();
			}
			catch (SignatureNotCorrectException e)
			{
				MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				er.Close();
			}
		}

		public string GetSaveDefaultFileFilter()
		{
			return "Nitro Kart Map Data (*.nkm)|*.nkm";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			int NrSections = 0;
			if (ObjectInformation != null) NrSections++;
			if (Path != null) NrSections++;
			if (Point != null) NrSections++;
			if (Stage != null) NrSections++;
			if (KartPointStart != null) NrSections++;
			if (KartPointJugem != null) NrSections++;
			if (KartPointSecond != null) NrSections++;
			if (KartPointCannon != null) NrSections++;
			if (KartPointMission != null) NrSections++;
			if (CheckPoint != null) NrSections++;
			if (CheckPointPath != null) NrSections++;
			if (ItemPoint != null) NrSections++;
			if (ItemPath != null) NrSections++;
			if (EnemyPoint != null) NrSections++;
			if (EnemyPath != null) NrSections++;
			if (MiniGameEnemyPoint != null) NrSections++;
			if (MiniGameEnemyPath != null) NrSections++;
			if (Area != null) NrSections++;
			if (Camera != null) NrSections++;
			Header.SectionOffsets = new UInt32[NrSections];
			Header.Write(er);

			int SectionIdx = 0;
			if (ObjectInformation != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				ObjectInformation.Write(er);
				SectionIdx++;
			}
			if (Path != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				Path.Write(er);
				SectionIdx++;
			}
			if (Point != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				Point.Write(er);
				SectionIdx++;
			}
			if (Stage != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				Stage.Write(er);
				SectionIdx++;
			}
			if (KartPointStart != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				KartPointStart.Write(er);
				SectionIdx++;
			}
			if (KartPointJugem != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				KartPointJugem.Write(er);
				SectionIdx++;
			}
			if (KartPointSecond != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				KartPointSecond.Write(er);
				SectionIdx++;
			}
			if (KartPointCannon != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				KartPointCannon.Write(er);
				SectionIdx++;
			}
			if (KartPointMission != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				KartPointMission.Write(er);
				SectionIdx++;
			}
			if (CheckPoint != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				CheckPoint.Write(er);
				SectionIdx++;
			}
			if (CheckPointPath != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				CheckPointPath.Write(er);
				SectionIdx++;
			}
			if (ItemPoint != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				ItemPoint.Write(er);
				SectionIdx++;
			}
			if (ItemPath != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				ItemPath.Write(er);
				SectionIdx++;
			}
			if (EnemyPoint != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				EnemyPoint.Write(er);
				SectionIdx++;
			}
			if (EnemyPath != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				EnemyPath.Write(er);
				SectionIdx++;
			}
			if (MiniGameEnemyPoint != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				MiniGameEnemyPoint.Write(er);
				SectionIdx++;
			}
			if (MiniGameEnemyPath != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				MiniGameEnemyPath.Write(er);
				SectionIdx++;
			}
			if (Area != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				Area.Write(er);
				SectionIdx++;
			}
			if (Camera != null)
			{
				WriteHeaderInfo(er, SectionIdx);
				Camera.Write(er);
				SectionIdx++;
			}

			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		private void WriteHeaderInfo(EndianBinaryWriter er, int Index)
		{
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = 8 + Index * 4;
			er.Write((UInt32)(curpos - Header.HeaderSize));
			er.BaseStream.Position = curpos;
		}

		public Form GetDialog()
		{
			return new NKMDViewer2(this);
		}

		public NKMDHeader Header;
		public class NKMDHeader
		{
			public NKMDHeader()
			{
				Signature = "NKMD";
				Version = 37;
				HeaderSize = 8;
				SectionOffsets = new uint[0];
			}

			public NKMDHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				SectionOffsets = er.ReadUInt32s((HeaderSize - 8) / 4);
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Version);
				HeaderSize = (UInt16)(8 + (SectionOffsets.Length * 4));
				er.Write(HeaderSize);
				er.Write(SectionOffsets, 0, SectionOffsets.Length);
			}
			[BinaryStringSignature("NKMD")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt16 Version;
			public UInt16 HeaderSize;
			[BinaryIgnore]
			public UInt32[] SectionOffsets;
		}

		public OBJI ObjectInformation;
		public PATH Path;
		public POIT Point;
		public STAG Stage;
		public KTPS KartPointStart;
		public KTPJ KartPointJugem;
		public KTP2 KartPointSecond;
		public KTPC KartPointCannon;
		public KTPM KartPointMission;
		public CPOI CheckPoint;
		public CPAT CheckPointPath;
		public IPOI ItemPoint;
		public IPAT ItemPath;
		public EPOI EnemyPoint;
		public EPAT EnemyPath;
		public MEPO MiniGameEnemyPoint;
		public MEPA MiniGameEnemyPath;
		public AREA Area;
		public CAME Camera;

		public class NKMDIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "Mario Kart DS";
			}

			public override string GetFileDescription()
			{
				return "Mario Kart DS Map Data (NKM)";
			}

			public override string GetFileFilter()
			{
				return "Mario Kart DS Map Data (*.nkm)|*.nkm";
			}

			public override Bitmap GetIcon()
			{
				return Resource.Cone;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'N' && File.Data[1] == 'K' && File.Data[2] == 'M' && File.Data[3] == 'D') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
