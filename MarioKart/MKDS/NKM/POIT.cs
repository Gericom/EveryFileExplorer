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
	public class POIT : GameDataSection<POIT.POITEntry>
	{
		public POIT() { Signature = "POIT"; }
		public POIT(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "POIT") throw new SignatureNotCorrectException(Signature, "POIT", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new POITEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"Index",
					"Duration",
					"?"
				};
		}

		public class POITEntry : GameDataSectionEntry
		{
			public POITEntry()
			{
				Position = new Vector3(0, 0, 0);
				Index = 0;
				Duration = 0;
				Unknown = 0;
			}
			public POITEntry(EndianBinaryReader er)
			{
				Position = er.ReadVecFx32();
				Index = er.ReadInt16();
				Duration = er.ReadInt16();
				Unknown = er.ReadUInt32();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(Index.ToString());
				m.SubItems.Add(Duration.ToString());
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown));
				return m;
			}
			[Category("Transformation")]
			public Vector3 Position { get; set; }
			[Category("Point")]
			public Int16 Index { get; set; }
			[Category("Point")]
			public Int16 Duration { get; set; }
			[Category("Point")]
			[TypeConverter(typeof(HexTypeConverter)), HexReversedAttribute]
			public UInt32 Unknown { get; set; }
		}
	}
}
