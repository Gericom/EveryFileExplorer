using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.GameData;
using System.IO;
using LibEveryFileExplorer.Collections;
using System.Windows.Forms;
using MarioKart.UI;
using LibEveryFileExplorer;

namespace MarioKart.MK7.KMP
{
	public class CDMD : FileFormat<CDMD.CDMDIdentifier>, IConvertable, IViewable
	{
		public CDMD(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CDMDHeader(er);
				foreach (var v in Header.SectionOffsets)
				{
					er.BaseStream.Position = Header.HeaderSize + v;
					String sig = er.ReadString(Encoding.ASCII, 4);
					er.BaseStream.Position -= 4;
					switch (sig)
					{
						case "TPKC": CheckPoint = new CKPT(er); break;
						case "HPKC": CheckPointPath = new CKPH(er); break;
						case "JBOG": GlobalObject = new GOBJ(er); break;
						case "TPGJ": JugemPoint = new JGPT(er); break;
						case "TPLG": GliderPoint = new GLPT(er); break;
						case "HPLG": GliderPointPath = new GLPH(er); break;
						default:
							//throw new Exception("Unknown Section: " + sig);
							continue;
						//goto cont;
					}
				}
			cont:
				;
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
			return new CDMDViewer(this);
		}

		public string GetConversionFileFilters()
		{
			return MKDS.NKM.NKMD.Identifier.GetFileFilter();
		}

		public bool Convert(int FilterIndex, string Path)
		{
			switch (FilterIndex)
			{
				case 0:
					MKDS.NKM.NKMD o = ToNKMD();
					byte[] d = o.Write();
					File.Create(Path).Close();
					File.WriteAllBytes(Path, d);
					return true;
				default:
					return false;
			}
		}

		public CDMDHeader Header;
		public class CDMDHeader
		{
			public CDMDHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "DMDC") throw new SignatureNotCorrectException(Signature, "DMDC", er.BaseStream.Position - 4);
				FileSize = er.ReadUInt32();
				NrSections = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				SectionOffsets = er.ReadUInt32s(NrSections);
			}
			public String Signature;
			public UInt32 FileSize;
			public UInt16 NrSections;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32[] SectionOffsets;
		}

		public CKPT CheckPoint;
		public CKPH CheckPointPath;

		public GOBJ GlobalObject;

		public JGPT JugemPoint;

		public GLPT GliderPoint;
		public GLPH GliderPointPath;

		public MKDS.NKM.NKMD ToNKMD()
		{
			MKDS.NKM.NKMD n = new MKDS.NKM.NKMD();
			n.ObjectInformation = new MKDS.NKM.OBJI();
			n.Path = new MKDS.NKM.PATH();
			n.Point = new MKDS.NKM.POIT();
			n.KartPointStart = new MKDS.NKM.KTPS();
			n.KartPointJugem = new MKDS.NKM.KTPJ();
			n.KartPointSecond = new MKDS.NKM.KTP2();
			n.KartPointCannon = new MKDS.NKM.KTPC();
			n.KartPointMission = new MKDS.NKM.KTPM();
			n.CheckPoint = new MKDS.NKM.CPOI();
			n.CheckPointPath = new MKDS.NKM.CPAT();
			n.ItemPoint = new MKDS.NKM.IPOI();
			n.ItemPath = new MKDS.NKM.IPAT();
			n.EnemyPoint = new MKDS.NKM.EPOI();
			n.EnemyPath = new MKDS.NKM.EPAT();
			//n.MiniGameEnemyPoint = new MKDS.NKM.MEPO();
			//n.MiniGameEnemyPath = new MKDS.NKM.MEPA();
			n.Area = new MKDS.NKM.AREA();
			n.Camera = new MKDS.NKM.CAME();

			foreach (var v in GlobalObject.Entries)
			{
				if (v.ObjectID == 0x012D)
				{
					n.KartPointStart.Entries.Add(new MKDS.NKM.KTPS.KTPSEntry()
					{
						Position = v.Position,
						Rotation = v.Rotation,
						Unknown = 0xFFFF,
						Index = -1
					});
					n.KartPointSecond.Entries.Add(new MKDS.NKM.KTP2.KTP2Entry()
					{
						Position = v.Position,
						Rotation = v.Rotation,
						Unknown = 0xFFFF,
						Index = -1
					});
					break;
				}
			}
			int i = 0;
			foreach (var v in JugemPoint.Entries)
			{
				n.KartPointJugem.Entries.Add(new MKDS.NKM.KTPJ.KTPJEntry()
				{
					Position = v.Position,
					Rotation = v.Rotation,
					ItemPointID = 0,
					EnemyPointID = 0,
					Index = i++
				});
			}
			i = 0;
			foreach (var v in CheckPoint.Entries)
			{
				var q = new MKDS.NKM.CPOI.CPOIEntry()
				{
					Point1 = v.Point1,
					Point2 = v.Point2,
					KeyPointID = v.Type,
					RespawnID = v.RespawnId,
				};

				//Tempoarly
				if (i == 0) q.StartSection = 0;
				else if (i == CheckPoint.Entries.Count - 1) q.GotoSection = 0;

				//TODO!
				/*if (v.Previous == 255)//start new section
				{
					
				}*/
				q.UpdateSinCos();
				n.CheckPoint.Entries.Add(q);
				i++;
			}
			foreach (var v in CheckPointPath.Entries)
			{
				var q = new MKDS.NKM.CPAT.CPATEntry()
				{
					StartIndex = v.Start,
					Length = v.Length,
					SectionOrder = 0//tempoarly
				};
				q.ComesFrom[0] = v.Previous[0];
				q.ComesFrom[1] = v.Previous[1];
				q.ComesFrom[2] = v.Previous[2];

				q.GoesTo[0] = v.Next[0];
				q.GoesTo[1] = v.Next[1];
				q.GoesTo[2] = v.Next[2];
				n.CheckPointPath.Entries.Add(q);
			}
			return n;
		}

		public class CDMDIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "Mario Kart 7";
			}

			public override string GetFileDescription()
			{
				return "CTR Dash Map Data (KMP)";
			}

			public override string GetFileFilter()
			{
				return "CTR Dash Map Data (*.kmp)|*.kmp";
			}

			public override Bitmap GetIcon()
			{
				return Resource.Cone;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'D' && File.Data[1] == 'M' && File.Data[2] == 'D' && File.Data[3] == 'C') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
