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
	public class PATH : GameDataSection<PATH.PATHEntry>
	{
		public PATH() { Signature = "PATH"; }
		public PATH(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "PATH") throw new SignatureNotCorrectException(Signature, "PATH", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new PATHEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"Index",
					"Loop",
					"Nr Points"
				};
		}

		public class PATHEntry : GameDataSectionEntry
		{
			public PATHEntry()
			{
				Index = 0;
				Loop = false;
				NrPoit = 0;
			}
			public PATHEntry(EndianBinaryReader er)
			{
				Index = er.ReadByte();
				Loop = er.ReadByte() == 1;
				NrPoit = er.ReadInt16();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Index.ToString());
				m.SubItems.Add(Loop.ToString());
				m.SubItems.Add(NrPoit.ToString());
				return m;
			}
			[Category("Path")]
			public Byte Index { get; set; }
			[Category("Path")]
			[Description("Specifies whether this route loops or not.")]
			public Boolean Loop { get; set; }
			[Category("Path"), DisplayName("Nr Poit")]
			[Description("The number of POIT entries that belong to this route.")]
			public Int16 NrPoit { get; set; }
		}
	}
}
