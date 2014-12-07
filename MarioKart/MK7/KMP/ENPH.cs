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
	public class ENPH : GameDataSection<ENPH.ENPHEntry>
	{
		public ENPH() { Signature = "HPNE"; }
		public ENPH(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "HPNE") throw new SignatureNotCorrectException(Signature, "HPNE", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new ENPHEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"Start",
					"Length",
					"Previous 1","Previous 2","Previous 3","Previous 4","Previous 5","Previous 6","Previous 7","Previous 8",
					"Previous 9","Previous 10","Previous 11","Previous 12","Previous 13","Previous 14","Previous 15","Previous 16",
					"Next 1","Next 2","Next 3","Next 4","Next 5","Next 6","Next 7","Next 8",
					"Next 9","Next 10","Next 11","Next 12","Next 13","Next 14","Next 15","Next 16",
					"?"
				};
		}
		public class ENPHEntry : GameDataSectionEntry
		{
			public ENPHEntry()
			{
				Previous = new short[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
				Next = new short[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
			}
			public ENPHEntry(EndianBinaryReader er)
			{
				Start = er.ReadUInt16();
				Length = er.ReadUInt16();
				Previous = er.ReadInt16s(16);
				Next = er.ReadInt16s(16);
				Unknown = er.ReadUInt32();
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
				m.SubItems.Add(Previous[6].ToString());
				m.SubItems.Add(Previous[7].ToString());
				m.SubItems.Add(Previous[8].ToString());
				m.SubItems.Add(Previous[9].ToString());
				m.SubItems.Add(Previous[10].ToString());
				m.SubItems.Add(Previous[11].ToString());
				m.SubItems.Add(Previous[12].ToString());
				m.SubItems.Add(Previous[13].ToString());
				m.SubItems.Add(Previous[14].ToString());
				m.SubItems.Add(Previous[15].ToString());
				m.SubItems.Add(Next[0].ToString());
				m.SubItems.Add(Next[1].ToString());
				m.SubItems.Add(Next[2].ToString());
				m.SubItems.Add(Next[3].ToString());
				m.SubItems.Add(Next[4].ToString());
				m.SubItems.Add(Next[5].ToString());
				m.SubItems.Add(Next[6].ToString());
				m.SubItems.Add(Next[7].ToString());
				m.SubItems.Add(Next[8].ToString());
				m.SubItems.Add(Next[9].ToString());
				m.SubItems.Add(Next[10].ToString());
				m.SubItems.Add(Next[11].ToString());
				m.SubItems.Add(Next[12].ToString());
				m.SubItems.Add(Next[13].ToString());
				m.SubItems.Add(Next[14].ToString());
				m.SubItems.Add(Next[15].ToString());

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown));
				return m;
			}
			public UInt16 Start { get; set; }
			public UInt16 Length { get; set; }
			public Int16[] Previous { get; set; }//16
			public Int16[] Next { get; set; }//16
			public UInt32 Unknown { get; set; }
		}
	}
}