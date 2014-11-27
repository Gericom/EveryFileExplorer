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

namespace MarioKart.MK7
{
	public class CDMD : FileFormat<CDMD.CDMDIdentifier>, IViewable
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
						case "TPKC": CheckPoints = new CKPT(er); break;
						case "HPKC": CheckPaths = new CKPH(er); break;
						case "JBOG": GlobalObjects = new GOBJ(er); break;
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

		public CKPT CheckPoints;
		public class CKPT : GameDataSection<CKPT.CKPTEntry>
		{
			public CKPT() { Signature = "TPKC"; }
			public CKPT(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "TPKC") throw new SignatureNotCorrectException(Signature, "TPKC", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new CKPTEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X1", "Z1",
					"X2", "Z2",
					"Respawn Id",
					"Type",
					"Previous",
					"Next",
					"?",
					"?",
					"?",
					"?"
				};
			}
			public class CKPTEntry : GameDataSectionEntry
			{
				public CKPTEntry()
				{
					Type = 0xFF;
					Unknown2 = 0xFF;
				}
				public CKPTEntry(EndianBinaryReader er)
				{
					Point1 = er.ReadVector2();
					Point2 = er.ReadVector2();
					RespawnId = er.ReadByte();
					Type = er.ReadByte();
					Previous = er.ReadByte();
					Next = er.ReadByte();
					Unknown1 = er.ReadByte();
					Unknown2 = er.ReadByte();
					Unknown3 = er.ReadByte();
					Unknown4 = er.ReadByte();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Point1.X.ToString("#####0.############"));
					m.SubItems.Add(Point1.Y.ToString("#####0.############"));

					m.SubItems.Add(Point2.X.ToString("#####0.############"));
					m.SubItems.Add(Point2.Y.ToString("#####0.############"));

					m.SubItems.Add(RespawnId.ToString());
					m.SubItems.Add(Type.ToString());
					m.SubItems.Add(Previous.ToString());
					m.SubItems.Add(Next.ToString());

					m.SubItems.Add(string.Format("{0:X2}", Unknown1));
					m.SubItems.Add(string.Format("{0:X2}", Unknown2));
					m.SubItems.Add(string.Format("{0:X2}", Unknown3));
					m.SubItems.Add(string.Format("{0:X2}", Unknown4));
					return m;
				}
				public Vector2 Point1 { get; set; }
				public Vector2 Point2 { get; set; }
				public Byte RespawnId { get; set; }
				public Byte Type { get; set; }
				public Byte Previous { get; set; }
				public Byte Next { get; set; }
				public Byte Unknown1 { get; set; }
				public Byte Unknown2 { get; set; }
				public Byte Unknown3 { get; set; }
				public Byte Unknown4 { get; set; }
			}
		}

		public CKPH CheckPaths;
		public class CKPH : GameDataSection<CKPH.CKPHEntry>
		{
			public CKPH() { Signature = "HPKC"; }
			public CKPH(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "HPKC") throw new SignatureNotCorrectException(Signature, "HPKC", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new CKPHEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"Start",
					"Length",
					"Previous 1","Previous 2","Previous 3","Previous 4","Previous 5","Previous 6",
					"Next 1","Next 2","Next 3","Next 4","Next 5","Next 6",
					"?"
				};
			}
			public class CKPHEntry : GameDataSectionEntry
			{
				public CKPHEntry()
				{
					Previous = new byte[6];
					Next = new byte[6];
				}
				public CKPHEntry(EndianBinaryReader er)
				{
					Start = er.ReadByte();
					Length = er.ReadByte();
					Previous = er.ReadBytes(6);
					Next = er.ReadBytes(6);
					Unknown = er.ReadUInt16();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Start.ToString());
					m.SubItems.Add(Length.ToString());
					m.SubItems.Add(Previous[0].ToString());
					m.SubItems.Add(Previous[1].ToString());
					m.SubItems.Add(Previous[2].ToString());
					m.SubItems.Add(Previous[3].ToString());
					m.SubItems.Add(Previous[4].ToString());
					m.SubItems.Add(Previous[5].ToString());
					m.SubItems.Add(Next[0].ToString());
					m.SubItems.Add(Next[1].ToString());
					m.SubItems.Add(Next[2].ToString());
					m.SubItems.Add(Next[3].ToString());
					m.SubItems.Add(Next[4].ToString());
					m.SubItems.Add(Next[5].ToString());

					m.SubItems.Add(HexUtil.GetHexReverse(Unknown));
					return m;
				}
				public Byte Start { get; set; }
				public Byte Length { get; set; }
				public Byte[] Previous { get; set; }//6
				public Byte[] Next { get; set; }//6
				public UInt16 Unknown { get; set; }
			}
		}

		public GOBJ GlobalObjects;
		public class GOBJ : GameDataSection<GOBJ.GOBJEntry>
		{
			public GOBJ() { Signature = "JBOG"; }
			public GOBJ(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "JBOG") throw new SignatureNotCorrectException(Signature, "JBOG", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new GOBJEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"Object ID",
					"?",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"X Scale", "Y Scale", "Z Scale",
					"Route ID",
					"Setting 1", "Setting 2", "Setting 3", "Setting 4", "Setting 5", "Setting 6", "Setting 7", "Setting 8",
					"Visibility",
					"?",
					"?"
				};
			}
			public class GOBJEntry : GameDataSectionEntry
			{
				public GOBJEntry()
				{
					ObjectID = 5;//coin
					Unknown1 = 0;
					Position = new Vector3(0, 0, 0);
					Rotation = new Vector3(0, 0, 0);
					Scale = new Vector3(1, 1, 1);
					RouteID = -1;
					Settings = new ushort[8];
					Visibility = 0;
					Unknown2 = 0xFFFF;
					Unknown3 = 0;
				}
				public GOBJEntry(EndianBinaryReader er)
				{
					ObjectID = er.ReadUInt16();
					Unknown1 = er.ReadUInt16();
					Position = er.ReadVector3();
					Rotation = er.ReadVector3();
					Scale = er.ReadVector3();
					RouteID = er.ReadInt16();
					Settings = er.ReadUInt16s(8);
					Visibility = er.ReadUInt16();
					Unknown2 = er.ReadUInt16();
					Unknown3 = er.ReadUInt16();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					//ObjectDb.Object ob = MK7_Const.ObjectDatabase.GetObject(o.ObjectID);
					//if (ob != null) i.SubItems.Add(ob.ToString());
					/*else */
					m.SubItems.Add(ToString());//String.Format("{0:X4}", ObjectID));
					m.SubItems.Add(HexUtil.GetHexReverse(Unknown1));
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Rotation.X.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

					m.SubItems.Add(Scale.X.ToString("#####0.############"));
					m.SubItems.Add(Scale.Y.ToString("#####0.############"));
					m.SubItems.Add(Scale.Z.ToString("#####0.############"));

					m.SubItems.Add(RouteID.ToString());

					m.SubItems.Add(HexUtil.GetHexReverse(Settings[0]));
					m.SubItems.Add(HexUtil.GetHexReverse(Settings[1]));
					m.SubItems.Add(HexUtil.GetHexReverse(Settings[2]));
					m.SubItems.Add(HexUtil.GetHexReverse(Settings[3]));
					m.SubItems.Add(HexUtil.GetHexReverse(Settings[4]));
					m.SubItems.Add(HexUtil.GetHexReverse(Settings[5]));
					m.SubItems.Add(HexUtil.GetHexReverse(Settings[6]));
					m.SubItems.Add(HexUtil.GetHexReverse(Settings[7]));

					m.SubItems.Add(HexUtil.GetHexReverse(Visibility));

					m.SubItems.Add(HexUtil.GetHexReverse(Unknown2));
					m.SubItems.Add(HexUtil.GetHexReverse(Unknown3));
					return m;
				}

				public UInt16 ObjectID { get; set; }
				public UInt16 Unknown1 { get; set; }
				public Vector3 Position { get; set; }
				public Vector3 Rotation { get; set; }
				public Vector3 Scale { get; set; }
				public Int16 RouteID { get; set; }
				public UInt16[] Settings { get; set; }//8
				public UInt16 Visibility { get; set; }
				public UInt16 Unknown2 { get; set; }
				public UInt16 Unknown3 { get; set; }

				public override string ToString()
				{
					if (ObjectID < MK7.ObjectNames.Length) return MK7.ObjectNames[ObjectID] + " (" + String.Format("{0:X4}", ObjectID) + ")";
					return "? (" + String.Format("{0:X4}", ObjectID) + ")";
				}
			}
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
