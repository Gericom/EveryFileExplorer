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
using LibEveryFileExplorer.Math;

namespace MarioKart.MKDS.NKM
{
	public class AREA : GameDataSection<AREA.AREAEntry>
	{
		public AREA() { Signature = "AREA"; }
		public AREA(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "AREA") throw new SignatureNotCorrectException(Signature, "AREA", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new AREAEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Vector Length", "Y Vector Length", "Z Vector Length",
					"X Vector X", "X Vector Y", "X Vector Z",
					"Y Vector X", "Y Vector Y", "Y Vector Z",
					"Z Vector X", "Z Vector Y", "Z Vector Z",
					"?",
					"?",
					"?",
					"?",
					"Linked CAME",
					"?",
					"?",
					"?"
				};
		}

		public class AREAEntry : GameDataSectionEntry
		{
			public AREAEntry()
			{

			}
			public AREAEntry(EndianBinaryReader er)
			{
				Position = er.ReadVecFx32();
				LengthVector = er.ReadVecFx32();
				XVector = er.ReadVecFx32();
				YVector = er.ReadVecFx32();
				ZVector = er.ReadVecFx32();

				Unknown5 = er.ReadInt16();
				Unknown6 = er.ReadInt16();
				Unknown7 = er.ReadInt16();

				Unknown8 = er.ReadByte();
				LinkedCame = er.ReadSByte();
				Unknown9 = er.ReadByte();

				Unknown10 = er.ReadUInt16();

				Unknown11 = er.ReadByte();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(LengthVector.X.ToString("#####0.############"));
				m.SubItems.Add(LengthVector.Y.ToString("#####0.############"));
				m.SubItems.Add(LengthVector.Z.ToString("#####0.############"));

				m.SubItems.Add(XVector.X.ToString("#####0.############"));
				m.SubItems.Add(XVector.Y.ToString("#####0.############"));
				m.SubItems.Add(XVector.Z.ToString("#####0.############"));

				m.SubItems.Add(YVector.X.ToString("#####0.############"));
				m.SubItems.Add(YVector.Y.ToString("#####0.############"));
				m.SubItems.Add(YVector.Z.ToString("#####0.############"));

				m.SubItems.Add(ZVector.X.ToString("#####0.############"));
				m.SubItems.Add(ZVector.Y.ToString("#####0.############"));
				m.SubItems.Add(ZVector.Z.ToString("#####0.############"));

				m.SubItems.Add(Unknown5.ToString());
				m.SubItems.Add(Unknown6.ToString());
				m.SubItems.Add(Unknown7.ToString());

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown8));
				m.SubItems.Add(LinkedCame.ToString());
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown9));

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown10));

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown11));
				return m;
			}

			[Category("Transformation")]
			public Vector3 Position { get; set; }

			[Category("Vectors"), DisplayName("Length Vector")]
			public Vector3 LengthVector { get; set; }
			[Category("Vectors"), DisplayName("X Vector")]
			public Vector3 XVector { get; set; }
			[Category("Vectors"), DisplayName("Y Vector")]
			public Vector3 YVector { get; set; }
			[Category("Vectors"), DisplayName("Z Vector")]
			public Vector3 ZVector { get; set; }

			public Int16 Unknown5 { get; set; }
			public Int16 Unknown6 { get; set; }
			public Int16 Unknown7 { get; set; }

			public Byte Unknown8 { get; set; }
			public SByte LinkedCame { get; set; }
			public Byte Unknown9 { get; set; }

			public UInt16 Unknown10 { get; set; }//unverified

			public Byte Unknown11 { get; set; }

			public Vector3[] GetCube()
			{
				Vector3 XVec = XVector * LengthVector.X * 100f;
				Vector3 YVec = YVector * LengthVector.Y * 100f;
				Vector3 ZVec = ZVector * LengthVector.Z * 100f;

				Vector3 BasePoint = Position - (XVec / 2) -  (YVec / 2) - (ZVec / 2);

				Vector3 XPoint = BasePoint + XVec;
				Vector3 YPoint = BasePoint + YVec;
				Vector3 ZPoint = BasePoint + ZVec;

				Vector3 XYPoint = BasePoint + XVec + YVec;
				Vector3 XZPoint = BasePoint + XVec + ZVec;
				Vector3 YZPoint = BasePoint + YVec + ZVec;

				Vector3 XYZPoint = BasePoint + XVec + YVec + ZVec;

				return new Vector3[] { BasePoint, XPoint, YPoint, ZPoint, XYPoint, XZPoint, YZPoint, XYZPoint };
			}
		}
	}
}
