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
	public class IPOI : GameDataSection<IPOI.IPOIEntry>
	{
		public IPOI() { Signature = "IPOI"; }
		public IPOI(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "IPOI") throw new SignatureNotCorrectException(Signature, "IPOI", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new IPOIEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"?",
					"?"
				};
		}

		public class IPOIEntry : GameDataSectionEntry
		{
			public IPOIEntry()
			{

			}
			public IPOIEntry(EndianBinaryReader er)
			{
				Position = er.ReadVecFx32();
				Unknown1 = er.ReadUInt32();
				Unknown2 = er.ReadUInt32();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown1));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown2));
				return m;
			}
			[Category("Transformation")]
			public Vector3 Position { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversed]
			public UInt32 Unknown1 { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversed]
			public UInt32 Unknown2 { get; set; }
		}
	}
}
