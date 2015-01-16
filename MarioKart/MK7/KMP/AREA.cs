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
using LibEveryFileExplorer.IO;

namespace MarioKart.MK7.KMP
{
	public class AREA : GameDataSection<AREA.AREAEntry>
	{
		public AREA() { Signature = "AERA"; }
		public AREA(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "AERA") throw new SignatureNotCorrectException(Signature, "AERA", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new AREAEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"Mode",
					"Type",
					"CAME",
					"?",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"X Scale", "Y Scale", "Z Scale",
					"?",
					"?",
					"?",
					"?",
				};
		}
		public class AREAEntry : GameDataSectionEntry
		{
			public AREAEntry()
			{

			}
			public AREAEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				Rotation = new Vector3(MathUtil.RadToDeg(Rotation.X), MathUtil.RadToDeg(Rotation.Y), MathUtil.RadToDeg(Rotation.Z));
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(HexUtil.GetHexReverse(Mode));
				m.SubItems.Add(HexUtil.GetHexReverse(Type));
				m.SubItems.Add(CAMEIndex.ToString());
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

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown2));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown3));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown4));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown5));
				return m;
			}
			public Byte Mode { get; set; }
			public Byte Type { get; set; }
			public SByte CAMEIndex { get; set; }
			public Byte Unknown1 { get; set; }
			public Vector3 Position { get; set; }
			public Vector3 Rotation { get; set; }
			public Vector3 Scale { get; set; }
			public UInt16 Unknown2 { get; set; }
			public UInt16 Unknown3 { get; set; }
			public UInt16 Unknown4 { get; set; }
			public UInt16 Unknown5 { get; set; }

			/*public Vector3[] GetCube()
			{
				Vector3 XVec = new Vector3(1000f * Scale.X, 0, 0);
				Vector3 YVec = new Vector3(0, 1000f * Scale.Y, 0);
				Vector3 ZVec = new Vector3(0, 0, 1000f * Scale.Z);

				Vector3 BasePoint = Position - (XVec / 2) - (YVec / 2) - (ZVec / 2);

				Vector3 XPoint = BasePoint + XVec;
				Vector3 YPoint = BasePoint + YVec;
				Vector3 ZPoint = BasePoint + ZVec;

				Vector3 XYPoint = BasePoint + XVec + YVec;
				Vector3 XZPoint = BasePoint + XVec + ZVec;
				Vector3 YZPoint = BasePoint + YVec + ZVec;

				Vector3 XYZPoint = BasePoint + XVec + YVec + ZVec;

				return new Vector3[] { BasePoint, XPoint, YPoint, ZPoint, XYPoint, XZPoint, YZPoint, XYZPoint };
			}*/
		}
	}
}
