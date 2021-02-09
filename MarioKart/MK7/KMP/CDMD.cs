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
using LibEveryFileExplorer.Math;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace MarioKart.MK7.KMP
{
	public class CDMD : FileFormat<CDMD.CDMDIdentifier>, IConvertable, IViewable
	{
		public CDMD(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
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
						case "TPTK": KartPoint = new KTPT(er); break;
						case "TPNE": EnemyPoint = new ENPT(er); break;
						case "HPNE": EnemyPointPath = new ENPH(er); break;
						case "TPTI": ItemPoint = new ITPT(er); break;
						case "HPTI": ItemPointPath = new ITPH(er); break;
						case "TPKC": CheckPoint = new CKPT(er); break;
						case "HPKC": CheckPointPath = new CKPH(er); break;
						case "JBOG": GlobalObject = new GOBJ(er); break;
						case "ITOP": PointInfo = new POTI(er); break;
						case "AERA": Area = new AREA(er); break;
						case "EMAC": Camera = new CAME(er); break;
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
			public CDMDHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				SectionOffsets = er.ReadUInt32s(NrSections);
			}
			[BinaryStringSignature("DMDC")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt32 FileSize;
			public UInt16 NrSections;
			public UInt16 HeaderSize;
			public UInt32 Version;
			[BinaryIgnore]
			public UInt32[] SectionOffsets;
		}

		public KTPT KartPoint;
		public ENPT EnemyPoint;
		public ENPH EnemyPointPath;
		public ITPT ItemPoint;
		public ITPH ItemPointPath;
		public CKPT CheckPoint;
		public CKPH CheckPointPath;
		public GOBJ GlobalObject;
		public POTI PointInfo;
		public AREA Area;
		public CAME Camera;
		public JGPT JugemPoint;
		//CNPT
		//MSPT
		//STGI
		//CORS
		public GLPT GliderPoint;
		public GLPH GliderPointPath;

		public MKDS.NKM.NKMD ToNKMD()
		{
			MKDS.NKM.NKMD n = new MKDS.NKM.NKMD();
			n.ObjectInformation = new MKDS.NKM.OBJI();
			n.Path = new MKDS.NKM.PATH();
			n.Point = new MKDS.NKM.POIT();
			n.Stage = new MKDS.NKM.STAG();
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
				if (v.ObjectID == 4)
				{
					n.ObjectInformation.Entries.Add(new MKDS.NKM.OBJI.OBJIEntry()
					{
						Position = v.Position,
						Rotation = v.Rotation,
						Scale = v.Scale,
						ObjectID = 0x65,
						RouteID = -1,
						TTVisible = true
					});
				}
				else if (v.ObjectID == 0x012D)
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
				}
			}
			int i = 0;
			foreach (var v in PointInfo.Routes)
			{
				n.Path.Entries.Add(new MKDS.NKM.PATH.PATHEntry()
				{
					NrPoit = (short)v.NrPoints,
					Loop = false,
					Index = (byte)i++
				});
				for (int ii = 0; ii < v.NrPoints; ii++)
				{
					n.Point.Entries.Add(new MKDS.NKM.POIT.POITEntry()
					{
						Position = v.Points[ii].Position,
						Duration = (short)v.Points[ii].Setting1,
						Index = (byte)ii
					});
				}
			}
			i = 0;
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
			foreach (var v in GliderPointPath.Entries)
			{
				var start = GliderPoint[v.Start];
				var end = GliderPoint[v.Start + v.Length - 1];
				n.KartPointCannon.Entries.Add(new MKDS.NKM.KTPC.KTPCEntry()
				{
					Position = end.Position,
					Rotation = new Vector3(0, (float)MathUtil.RadToDeg(Math.Atan2(end.Position.X - start.Position.X, end.Position.Z - start.Position.Z)), 0),
					NextMEPO = -1,
					Index = (short)i++
				});
			}
			i = 0;
			foreach (var v in CheckPoint.Entries)
			{
				var q = new MKDS.NKM.CPOI.CPOIEntry()
				{
					Point1 = v.Point1,
					Point2 = v.Point2,
					KeyPointID = (short)(v.Type == 255?-1:v.Type),
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
			foreach (var v in ItemPoint.Entries)
			{
				n.ItemPoint.Entries.Add(new MKDS.NKM.IPOI.IPOIEntry()
				{
					Position = v.Position
				});
			}
			foreach (var v in ItemPointPath.Entries)
			{
				var q = new MKDS.NKM.IPAT.IPATEntry()
				{
					StartIndex = (short)v.Start,
					Length = (short)v.Length
				};
				q.ComesFrom[0] = (byte)v.Previous[0];
				q.ComesFrom[1] = (byte)v.Previous[1];
				q.ComesFrom[2] = (byte)v.Previous[2];
				q.GoesTo[0] = (byte)v.Next[0];
				q.GoesTo[1] = (byte)v.Next[1];
				q.GoesTo[2] = (byte)v.Next[2];
				n.ItemPath.Entries.Add(q);
			}
			foreach (var v in EnemyPoint.Entries)
			{
				n.EnemyPoint.Entries.Add(new MKDS.NKM.EPOI.EPOIEntry()
				{
					Position = v.Position,
					PointSize = v.Unknown1 * 100f //?
				});
			}
			foreach (var v in EnemyPointPath.Entries)
			{
				var q = new MKDS.NKM.EPAT.EPATEntry()
				{
					StartIndex = (short)v.Start,
					Length = (short)v.Length
				};
				q.ComesFrom[0] = (byte)v.Previous[0];
				q.ComesFrom[1] = (byte)v.Previous[1];
				q.ComesFrom[2] = (byte)v.Previous[2];
				q.GoesTo[0] = (byte)v.Next[0];
				q.GoesTo[1] = (byte)v.Next[1];
				q.GoesTo[2] = (byte)v.Next[2];
				n.EnemyPath.Entries.Add(q);
			}
			bool first = true;
			foreach (var v in Camera.Entries)
			{
				if (v.Type != 5)
				{
					n.Camera.Entries.Add(new MKDS.NKM.CAME.CAMEEntry());
				}
				else
				{
					float begindist = (float)(400f / (2f * Math.Tan(MathUtil.DegToRad(v.FOVBegin) / 2f)));
					float beginFov = (float)Math.Atan(256f / (2f * begindist)) * 2f;

					float enddist = (float)(400f / (2f * Math.Tan(MathUtil.DegToRad(v.FOVEnd) / 2f)));
					float endFov = (float)Math.Atan(256f / (2f * enddist)) * 2f;

					int routespeed = 0;
					if (v.RouteID != 255)
					{
						for (int p = 0; p< PointInfo.Routes[v.RouteID].NrPoints; p++)
						{
							routespeed += PointInfo.Routes[v.RouteID].Points[p].Setting1;
						}
						routespeed /= PointInfo.Routes[v.RouteID].NrPoints;
					}

					var q = new MKDS.NKM.CAME.CAMEEntry()
					{
						Position = v.Position,
						Angle = v.Rotation,
						Viewpoint1 = v.Viewpoint1,
						Viewpoint2 = v.Viewpoint2,
						FieldOfViewBegin = (UInt16)MathUtil.RadToDeg(beginFov / 1.5f),//(v.FOVBegin / 1.25f),
						FieldOfViewEnd = (UInt16)MathUtil.RadToDeg(endFov / 1.5f),//(v.FOVEnd / 1.25f),
						FovSpeed = /*10*/(Int16)(v.FOVSpeed * 10),//v.Duration / 20),//(Int16)(v.FOVSpeed * 10),
						CameraType = 3,
						LinkedRoute = v.RouteID,
						RouteSpeed = (Int16)(routespeed * 2),//v.Duration / 2 / (v.RouteID != 255 ? PointInfo.Routes[v.RouteID].NrPoints : 1)),//10,//(short)(v.RouteID != 255 ? PointInfo.Routes[v.RouteID].Points[0].Setting1 / 16.67f : 0), //(Int16)v.RouteSpeed,
						PointSpeed = (Int16)(v.Duration / 10),//v.ViewpointSpeed / 45),//(Int16)(v.ViewpointSpeed / 16.67f),
						Duration = (Int16)v.Duration,
						NextCamera = v.Next,
						FirstIntroCamera = first ? MKDS.NKM.CAME.CAMEEntry.CAMEIntroCamera.Top : MKDS.NKM.CAME.CAMEEntry.CAMEIntroCamera.No
					};
					n.Camera.Entries.Add(q);
					first = false;
				}
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
				return "Mario Kart 7 Map Data (KMP)";
			}

			public override string GetFileFilter()
			{
				return "Mario Kart 7 Map Data (*.kmp)|*.kmp";
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
