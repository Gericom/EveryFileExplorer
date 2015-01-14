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
	public class ITPT : GameDataSection<ITPT.ITPTEntry>
	{
		public ITPT() { Signature = "TPTI"; }
		public ITPT(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "TPTI") throw new SignatureNotCorrectException(Signature, "TPTI", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new ITPTEntry(er));
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
		public class ITPTEntry : GameDataSectionEntry
		{
			public ITPTEntry() { }
			public ITPTEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString());
				m.SubItems.Add(Position.Y.ToString());
				m.SubItems.Add(Position.Z.ToString());

				m.SubItems.Add(Unknown1.ToString());
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown2));
				return m;
			}
			public Vector3 Position { get; set; }
			public Single Unknown1 { get; set; }
			public UInt32 Unknown2 { get; set; }
		}
	}
}
