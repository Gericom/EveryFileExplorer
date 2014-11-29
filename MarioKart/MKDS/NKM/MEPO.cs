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
	public class MEPO : GameDataSection<MEPO.MEPOEntry>
	{
		public MEPO() { Signature = "MEPO"; }
		public MEPO(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "MEPO") throw new SignatureNotCorrectException(Signature, "MEPO", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new MEPOEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"Point Size",
					"Drifting",
					"?"
				};
		}

		public class MEPOEntry : GameDataSectionEntry
		{
			public MEPOEntry()
			{

			}
			public MEPOEntry(EndianBinaryReader er)
			{
				Position = er.ReadVecFx32();
				PointSize = er.ReadFx32();
				Drifting = er.ReadInt16();
				Unknown1 = er.ReadUInt32();
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
				return m;
			}
			[Category("Transformation")]
			public Vector3 Position { get; set; }
			[Category("Enemy Point"), DisplayName("Point Size")]
			public Single PointSize { get; set; }
			[Category("Enemy Point")]
			public Int16 Drifting { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversed]
			public UInt32 Unknown1 { get; set; }
		}
	}
}
