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
	public class KTPM : GameDataSection<KTPM.KTPMEntry>
	{
		public KTPM() { Signature = "KTPM"; }
		public KTPM(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "KTPM") throw new SignatureNotCorrectException(Signature, "KTPM", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPMEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"?",
					"Index"
				};
		}

		public class KTPMEntry : GameDataSectionEntry
		{
			public KTPMEntry()
			{
				Position = new Vector3(0, 0, 0);
				Rotation = new Vector3(0, 0, 0);
				Unknown = 0xFFFF;
				Index = 1;
			}
			public KTPMEntry(EndianBinaryReader er)
			{
				Position = er.ReadVecFx32();
				Rotation = er.ReadVecFx32();
				Unknown = er.ReadUInt16();
				Index = er.ReadInt16();
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

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown));
				m.SubItems.Add(Index.ToString());
				return m;
			}
			[Category("Transformation")]
			public Vector3 Position { get; set; }
			[Category("Transformation")]
			public Vector3 Rotation { get; set; }
			[Category("Mission Point")]
			[TypeConverter(typeof(HexTypeConverter)), HexReversedAttribute]
			public UInt16 Unknown { get; set; }
			[Category("Mission Point")]
			public Int16 Index { get; set; }
		}
	}
}
