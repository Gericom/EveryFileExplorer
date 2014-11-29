using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.GameData;
using System.IO;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Collections;
using System.Windows.Forms;
using LibEveryFileExplorer;
using System.ComponentModel;
using LibEveryFileExplorer.ComponentModel;

namespace MarioKart.MKDS.NKM
{
	public class OBJI : GameDataSection<OBJI.OBJIEntry>
	{
		public OBJI() { Signature = "OBJI"; }
		public OBJI(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "OBJI") throw new SignatureNotCorrectException(Signature, "OBJI", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new OBJIEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"X Scale", "Y Scale", "Z Scale",
					"Object ID",
					"Route ID",
					"Setting 1", "Setting 2", "Setting 3", "Setting 4", "Setting 5", "Setting 6", "Setting 7", "Setting 8",
					"TT Visible"
				};
		}

		public class OBJIEntry : GameDataSectionEntry
		{
			public OBJIEntry()
			{
				Position = new Vector3(0, 0, 0);
				Rotation = new Vector3(0, 0, 0);
				Scale = new Vector3(1, 1, 1);
				ObjectID = 0x0065;//itembox
				RouteID = -1;
				Settings = new ushort[8];
				TTVisible = true;
			}
			public OBJIEntry(EndianBinaryReader er)
			{
				Position = er.ReadVecFx32();
				Rotation = er.ReadVecFx32();
				Scale = er.ReadVecFx32();
				ObjectID = er.ReadUInt16();
				RouteID = er.ReadInt16();
				Settings = er.ReadUInt16s(8);
				TTVisible = er.ReadUInt32() == 1;
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(Rotation.X.ToString("#####0.############"));
				m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
				m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

				m.SubItems.Add(Scale.X.ToString("#####0.############"));
				m.SubItems.Add(Scale.Y.ToString("#####0.############"));
				m.SubItems.Add(Scale.Z.ToString("#####0.############"));

				//ObjectDb.Object ob = MKDS_Const.ObjectDatabase.GetObject(o.ObjectID);
				//if (ob != null) i.SubItems.Add(ob.ToString());
				/*else */
				m.SubItems.Add(HexUtil.GetHexReverse(ObjectID));
				m.SubItems.Add(RouteID.ToString());

				m.SubItems.Add(HexUtil.GetHexReverse(Settings[0]));
				m.SubItems.Add(HexUtil.GetHexReverse(Settings[1]));
				m.SubItems.Add(HexUtil.GetHexReverse(Settings[2]));
				m.SubItems.Add(HexUtil.GetHexReverse(Settings[3]));
				m.SubItems.Add(HexUtil.GetHexReverse(Settings[4]));
				m.SubItems.Add(HexUtil.GetHexReverse(Settings[5]));
				m.SubItems.Add(HexUtil.GetHexReverse(Settings[6]));
				m.SubItems.Add(HexUtil.GetHexReverse(Settings[7]));

				m.SubItems.Add(TTVisible.ToString());
				return m;
			}
			[Category("Transformation")]
			public Vector3 Position { get; set; }
			[Category("Transformation")]
			public Vector3 Rotation { get; set; }
			[Category("Transformation")]
			public Vector3 Scale { get; set; }
			[Category("Object"), DisplayName("Object ID")]
			[TypeConverter(typeof(HexTypeConverter)), HexReversedAttribute]
			public UInt16 ObjectID { get; set; }
			[Category("Object"), DisplayName("Route ID")]
			[Description("The route used by this object. If no route is used, set this to -1.")]
			public Int16 RouteID { get; set; }
			[Category("Object")]
			[Description("Object specific settings.")]
			[TypeConverter(typeof(PrettyArrayConverter))]
			public UInt16[] Settings { get; private set; }//8
			[Category("Object"), DisplayName("TT Visible")]
			[Description("Specifies whether the object is visible in Time Trail mode or not.")]
			public Boolean TTVisible { get; set; }
		}
	}
}
