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

namespace MarioKart.MKDS.NKM
{
	public class NKMD : FileFormat<NKMD.NKMDIdentifier>, IViewable
	{
		public NKMD(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
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
						case "IPOI": ItemPoint = new IPOI(er); break;
						case "IPAT": ItemPath = new IPAT(er); break;
						case "EPOI": EnemyPoint = new EPOI(er); break;
						case "EPAT": EnemyPath = new EPAT(er); break;
						case "MEPO": MiniGameEnemyPoint = new MEPO(er); break;
						case "MEPA": MiniGameEnemyPath = new MEPA(er); break;
						case "AREA": Area = new AREA(er); break;
						case "CAME": Camera = new CAME(er); break;
						default:
							//throw new Exception("Unknown Section: " + sig);
							continue;
						//goto cont;
					}
				}
			cont:
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

		public Form GetDialog()
		{
			return new NKMDViewer(this);
		}

		public NKMDHeader Header;
		public class NKMDHeader
		{
			public NKMDHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "NKMD") throw new SignatureNotCorrectException(Signature, "NKMD", er.BaseStream.Position - 4);
				Version = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				SectionOffsets = er.ReadUInt32s((HeaderSize - 8) / 4);
			}
			public String Signature;
			public UInt16 Version;
			public UInt16 HeaderSize;
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
				return "Nitro Kart Map Data (NKM)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Kart Map Data (*.nkm)|*.nkm";
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
