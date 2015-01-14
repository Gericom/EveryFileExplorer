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
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace MarioKart.MKDS.NKM
{
	public class EPOI : GameDataSection<EPOI.EPOIEntry>
	{
		public EPOI() { Signature = "EPOI"; }
		public EPOI(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "EPOI") throw new SignatureNotCorrectException(Signature, "EPOI", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new EPOIEntry(er));
		}

		public void Write(EndianBinaryWriter er)
		{
			er.Write(Signature, Encoding.ASCII, false);
			NrEntries = (uint)Entries.Count;
			er.Write(NrEntries);
			for (int i = 0; i < NrEntries; i++) Entries[i].Write(er);
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"Point Size",
					"Drifting",
					"?",
					"?"
				};
		}

		public class EPOIEntry : GameDataSectionEntry
		{
			public EPOIEntry()
			{

			}
			public EPOIEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}

			public override void Write(EndianBinaryWriter er)
			{
				er.WriteVecFx32(Position);
				er.WriteFx32(PointSize);
				er.Write(Drifting);
				er.Write(Unknown1);
				er.Write(Unknown2);
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(PointSize.ToString("#####0.############"));

				m.SubItems.Add(Drifting.ToString());

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown1));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown2));
				return m;
			}
			[Category("Transformation")]
			[BinaryFixedPoint(true, 19, 12)]
			public Vector3 Position { get; set; }
			[Category("Enemy Point"), DisplayName("Point Size")]
			[BinaryFixedPoint(true, 19, 12)]
			public Single PointSize { get; set; }
			[Category("Enemy Point")]
			public Int16 Drifting { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversed]
			public UInt16 Unknown1 { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversed]
			public UInt32 Unknown2 { get; set; }
		}
	}
}
