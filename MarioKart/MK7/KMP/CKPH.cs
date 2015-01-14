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
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace MarioKart.MK7.KMP
{
	public class CKPH : GameDataSection<CKPH.CKPHEntry>
	{
		public CKPH() { Signature = "HPKC"; }
		public CKPH(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "HPKC") throw new SignatureNotCorrectException(Signature, "HPKC", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new CKPHEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"Start",
					"Length",
					"Previous 1","Previous 2","Previous 3","Previous 4","Previous 5","Previous 6",
					"Next 1","Next 2","Next 3","Next 4","Next 5","Next 6",
					"?"
				};
		}
		public class CKPHEntry : GameDataSectionEntry
		{
			public CKPHEntry()
			{
				Previous = new sbyte[] { -1, -1, -1, -1, -1, -1 };
				Next = new sbyte[] { -1, -1, -1, -1, -1, -1 };
			}
			public CKPHEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Start.ToString());
				m.SubItems.Add(Length.ToString());
				m.SubItems.Add(Previous[0].ToString());
				m.SubItems.Add(Previous[1].ToString());
				m.SubItems.Add(Previous[2].ToString());
				m.SubItems.Add(Previous[3].ToString());
				m.SubItems.Add(Previous[4].ToString());
				m.SubItems.Add(Previous[5].ToString());
				m.SubItems.Add(Next[0].ToString());
				m.SubItems.Add(Next[1].ToString());
				m.SubItems.Add(Next[2].ToString());
				m.SubItems.Add(Next[3].ToString());
				m.SubItems.Add(Next[4].ToString());
				m.SubItems.Add(Next[5].ToString());

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown));
				return m;
			}
			public Byte Start { get; set; }
			public Byte Length { get; set; }
			[BinaryFixedSize(6)]
			public SByte[] Previous { get; set; }//6
			[BinaryFixedSize(6)]
			public SByte[] Next { get; set; }//6
			public UInt16 Unknown { get; set; }
		}
	}
}
