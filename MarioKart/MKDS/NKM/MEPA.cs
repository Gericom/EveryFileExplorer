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
	public class MEPA : GameDataSection<MEPA.MEPAEntry>
	{
		public MEPA() { Signature = "MEPA"; }
		public MEPA(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "MEPA") throw new SignatureNotCorrectException(Signature, "MEPA", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new MEPAEntry(er));
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
					"Start",
					"Length",
					"Next",
					"Next",
					"Next",
					"Previous",
					"Previous",
					"Previous"
				};
		}

		public class MEPAEntry : GameDataSectionEntry
		{
			public MEPAEntry()
			{
				GoesTo = new sbyte[] { -1, -1, -1, -1, -1, -1, -1, -1 };
				ComesFrom = new sbyte[] { -1, -1, -1, -1, -1, -1, -1, -1 };
			}
			public MEPAEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}

			public override void Write(EndianBinaryWriter er)
			{
				er.Write(StartIndex);
				er.Write(Length);
				er.Write(GoesTo, 0, 8);
				er.Write(ComesFrom, 0, 8);
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(StartIndex.ToString());
				m.SubItems.Add(Length.ToString());

				m.SubItems.Add(GoesTo[0].ToString());
				m.SubItems.Add(GoesTo[1].ToString());
				m.SubItems.Add(GoesTo[2].ToString());
				m.SubItems.Add(GoesTo[3].ToString());
				m.SubItems.Add(GoesTo[4].ToString());
				m.SubItems.Add(GoesTo[5].ToString());
				m.SubItems.Add(GoesTo[6].ToString());
				m.SubItems.Add(GoesTo[7].ToString());

				m.SubItems.Add(ComesFrom[0].ToString());
				m.SubItems.Add(ComesFrom[1].ToString());
				m.SubItems.Add(ComesFrom[2].ToString());
				m.SubItems.Add(ComesFrom[3].ToString());
				m.SubItems.Add(ComesFrom[4].ToString());
				m.SubItems.Add(ComesFrom[5].ToString());
				m.SubItems.Add(ComesFrom[6].ToString());
				m.SubItems.Add(ComesFrom[7].ToString());
				return m;
			}
			[Category("Enemy Path"), DisplayName("Start Index")]
			public Int16 StartIndex { get; set; }
			[Category("Enemy Path")]
			public Int16 Length { get; set; }
			[Category("Enemy Path"), DisplayName("Goes To")]
			[TypeConverter(typeof(PrettyArrayConverter))]
			[BinaryFixedSize(8)]
			public SByte[] GoesTo { get; private set; }//8
			[Category("Enemy Path"), DisplayName("Comes From")]
			[TypeConverter(typeof(PrettyArrayConverter))]
			[BinaryFixedSize(8)]
			public SByte[] ComesFrom { get; private set; }//8
		}
	}
}
