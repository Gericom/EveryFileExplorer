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
	public class KTPT : GameDataSection<KTPT.KTPTEntry>
	{
		public KTPT() { Signature = "TPTK"; }
		public KTPT(EndianBinaryReaderEx er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "TPTK") throw new SignatureNotCorrectException(Signature, "TPTK", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPTEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"Index"
				};
		}
		public class KTPTEntry : GameDataSectionEntry
		{
			public KTPTEntry() { }
			public KTPTEntry(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				Rotation = new Vector3(MathUtil.RadToDeg(Rotation.X), MathUtil.RadToDeg(Rotation.Y), MathUtil.RadToDeg(Rotation.Z));
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString());
				m.SubItems.Add(Position.Y.ToString());
				m.SubItems.Add(Position.Z.ToString());

				m.SubItems.Add(Rotation.X.ToString());
				m.SubItems.Add(Rotation.Y.ToString());
				m.SubItems.Add(Rotation.Z.ToString());

				m.SubItems.Add(Index.ToString());
				return m;
			}
			public Vector3 Position { get; set; }
			public Vector3 Rotation { get; set; }
			public UInt32 Index { get; set; }
		}
	}
}
