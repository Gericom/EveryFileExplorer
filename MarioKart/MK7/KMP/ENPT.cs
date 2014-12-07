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

namespace MarioKart.MK7.KMP
{
	public class ENPT : GameDataSection<ENPT.ENPTEntry>
	{
		public ENPT() { Signature = "TPNE"; }
		public ENPT(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "TPNE") throw new SignatureNotCorrectException(Signature, "TPNE", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new ENPTEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"?",
					"?",
					"?"
				};
		}
		public class ENPTEntry : GameDataSectionEntry
		{
			public ENPTEntry() { }
			public ENPTEntry(EndianBinaryReader er)
			{
				Position = er.ReadVector3();
				Unknown1 = er.ReadSingle();
				Unknown2 = er.ReadUInt32();
				Unknown3 = er.ReadUInt32();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString());
				m.SubItems.Add(Position.Y.ToString());
				m.SubItems.Add(Position.Z.ToString());

				m.SubItems.Add(Unknown1.ToString());
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown2));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown3));
				return m;
			}
			public Vector3 Position { get; set; }
			public Single Unknown1 { get; set; }
			public UInt32 Unknown2 { get; set; }
			public UInt32 Unknown3 { get; set; }
		}
	}
}
