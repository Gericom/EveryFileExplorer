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
	public class IPOI : GameDataSection<IPOI.IPOIEntry>
	{
		public IPOI() { Signature = "IPOI"; }
		public IPOI(EndianBinaryReaderEx er, UInt16 Version)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "IPOI") throw new SignatureNotCorrectException(Signature, "IPOI", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new IPOIEntry(er, Version));
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
					"X", "Y", "Z",
					"?",
					"?"
				};
		}

		public class IPOIEntry : GameDataSectionEntry
		{
			private UInt16 Version = 37;
			public IPOIEntry()
			{

			}
			public IPOIEntry(EndianBinaryReaderEx er, UInt16 Version)
			{
				this.Version = Version;
				Position = er.ReadVecFx32();
				Unknown1 = er.ReadUInt32();
				if (Version >= 34) Unknown2 = er.ReadUInt32();
			}

			public override void Write(EndianBinaryWriter er)
			{
				er.WriteVecFx32(Position);
				er.Write(Unknown1);
				if (Version >= 34) er.Write(Unknown2);
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
			[BinaryFixedPoint(true, 19, 12)]
			public Vector3 Position { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversed]
			public UInt32 Unknown1 { get; set; }
			[TypeConverter(typeof(HexTypeConverter)), HexReversed]
			public UInt32 Unknown2 { get; set; }
		}
	}
}
