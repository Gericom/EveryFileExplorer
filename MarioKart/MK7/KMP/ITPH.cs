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

namespace MarioKart.MK7.KMP
{
	public class ITPH : GameDataSection<ITPH.ITPHEntry>
	{
		public ITPH() { Signature = "HPTI"; }
		public ITPH(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "HPTI") throw new SignatureNotCorrectException(Signature, "HPTI", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new ITPHEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"Start",
					"Length",
					"Previous 1","Previous 2","Previous 3","Previous 4","Previous 5","Previous 6",
					"Next 1","Next 2","Next 3","Next 4","Next 5","Next 6"
				};
		}
		public class ITPHEntry : GameDataSectionEntry
		{
			public ITPHEntry()
			{
				Previous = new short[] { -1, -1, -1, -1, -1, -1};
				Next = new short[] { -1, -1, -1, -1, -1, -1 };
			}
			public ITPHEntry(EndianBinaryReader er)
			{
				Start = er.ReadUInt16();
				Length = er.ReadUInt16();
				Previous = er.ReadInt16s(6);
				Next = er.ReadInt16s(6);
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
				return m;
			}
			public UInt16 Start { get; set; }
			public UInt16 Length { get; set; }
			public Int16[] Previous { get; set; }//6
			public Int16[] Next { get; set; }//6
		}
	}
}