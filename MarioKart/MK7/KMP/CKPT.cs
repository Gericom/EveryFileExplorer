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

namespace MarioKart.MK7.KMP
{
	public class CKPT : GameDataSection<CKPT.CKPTEntry>
	{
		public CKPT() { Signature = "TPKC"; }
		public CKPT(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "TPKC") throw new SignatureNotCorrectException(Signature, "TPKC", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new CKPTEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X1", "Z1",
					"X2", "Z2",
					"Respawn Id",
					"Type",
					"Previous",
					"Next",
					"?",
					"?",
					"?",
					"?"
				};
		}
		public class CKPTEntry : GameDataSectionEntry
		{
			public CKPTEntry()
			{
				Type = 0xFF;
				Unknown2 = 0xFF;
			}
			public CKPTEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Point1.X.ToString());
				m.SubItems.Add(Point1.Y.ToString());

				m.SubItems.Add(Point2.X.ToString());
				m.SubItems.Add(Point2.Y.ToString());

				m.SubItems.Add(RespawnId.ToString());
				m.SubItems.Add(Type.ToString());
				m.SubItems.Add(Previous.ToString());
				m.SubItems.Add(Next.ToString());

				m.SubItems.Add(HexUtil.GetHexReverse(Unknown1));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown2));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown3));
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown4));
				return m;
			}
			public Vector2 Point1 { get; set; }
			public Vector2 Point2 { get; set; }
			public Byte RespawnId { get; set; }
			public Byte Type { get; set; }
			public Byte Previous { get; set; }
			public Byte Next { get; set; }
			public Byte Unknown1 { get; set; }
			public Byte Unknown2 { get; set; }
			public Byte Unknown3 { get; set; }
			public Byte Unknown4 { get; set; }
		}
	}
}
