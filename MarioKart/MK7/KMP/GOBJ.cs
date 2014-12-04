using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;
using System.Windows.Forms;
using System.IO;
using LibEveryFileExplorer.GameData;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer;
using LibEveryFileExplorer.Math;

namespace MarioKart.MK7.KMP
{
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
				Rotation = new Vector3(MathUtil.RadToDeg(Rotation.X), MathUtil.RadToDeg(Rotation.Y), MathUtil.RadToDeg(Rotation.Z));
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
				m.SubItems.Add(Position.X.ToString());
				m.SubItems.Add(Position.Y.ToString());
				m.SubItems.Add(Position.Z.ToString());

				m.SubItems.Add(Rotation.X.ToString());
				m.SubItems.Add(Rotation.Y.ToString());
				m.SubItems.Add(Rotation.Z.ToString());

				m.SubItems.Add(Scale.X.ToString());
				m.SubItems.Add(Scale.Y.ToString());
				m.SubItems.Add(Scale.Z.ToString());

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
}
