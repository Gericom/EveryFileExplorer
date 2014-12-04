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
	public class CPOI : GameDataSection<CPOI.CPOIEntry>
	{
		public CPOI() { Signature = "CPOI"; }
		public CPOI(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "CPOI") throw new SignatureNotCorrectException(Signature, "CPOI", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new CPOIEntry(er));
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
					"X1", "Z1", "X2", "Z2",
					"Sine", "Cosine",
					"Distance",
					"Goto Section", "Start Section",
					"Key Point",
					"Respawn",
					"?"
				};
		}

		public class CPOIEntry : GameDataSectionEntry
		{
			public CPOIEntry()
			{
				UpdateSinCos();
				GotoSection = -1;
				StartSection = -1;
				KeyPointID = -1;
			}
			public CPOIEntry(EndianBinaryReader er)
			{
				Point1 = new Vector2(er.ReadFx32(), er.ReadFx32());
				Point2 = new Vector2(er.ReadFx32(), er.ReadFx32());
				Sine = er.ReadFx32();
				Cosine = er.ReadFx32();
				Distance = er.ReadFx32();
				GotoSection = er.ReadInt16();
				StartSection = er.ReadInt16();
				KeyPointID = er.ReadInt16();
				RespawnID = er.ReadByte();
				Unknown = er.ReadByte();
			}

			public override void Write(EndianBinaryWriter er)
			{
				er.WriteFx32(Point1.X);
				er.WriteFx32(Point1.Y);
				er.WriteFx32(Point2.X);
				er.WriteFx32(Point2.Y);
				UpdateSinCos();
				er.WriteFx32(Sine);
				er.WriteFx32(Cosine);
				er.WriteFx32(Distance);
				er.Write(GotoSection);
				er.Write(StartSection);
				er.Write(KeyPointID);
				er.Write(RespawnID);
				er.Write(Unknown);
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Point1.X.ToString("#####0.############"));
				m.SubItems.Add(Point1.Y.ToString("#####0.############"));

				m.SubItems.Add(Point2.X.ToString("#####0.############"));
				m.SubItems.Add(Point2.Y.ToString("#####0.############"));

				m.SubItems.Add(Sine.ToString("#####0.############"));
				m.SubItems.Add(Cosine.ToString("#####0.############"));
				m.SubItems.Add(Distance.ToString("#####0.############"));

				m.SubItems.Add(GotoSection.ToString());
				m.SubItems.Add(StartSection.ToString());
				m.SubItems.Add(KeyPointID.ToString());
				m.SubItems.Add(RespawnID.ToString());
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown));
				return m;
			}
			[Category("Points"), DisplayName("Point 1")]
			public Vector2 Point1 { get; set; }
			[Category("Points"), DisplayName("Point 2")]
			public Vector2 Point2 { get; set; }
			[Browsable(false)]
			public Single Sine { get; private set; }
			[Browsable(false)]
			public Single Cosine { get; private set; }
			[Browsable(false)]
			public Single Distance { get; private set; }
			[Category("Sections"), DisplayName("Goto Section")]
			[Description("Specifies if the next checkpoint is in a new section. Use -1 otherwise.")]
			public Int16 GotoSection { get; set; }
			[Category("Sections"), DisplayName("Start Section")]
			[Description("Specifies if a new section is started (including this checkpoint). Use -1 otherwise.")]
			public Int16 StartSection { get; set; }
			[Category("Checkpoint"), DisplayName("Key Point")]
			public Int16 KeyPointID { get; set; }
			[Category("Checkpoint"), DisplayName("Respawn")]
			public Byte RespawnID { get; set; }
			[Category("Checkpoint")]
			public Byte Unknown { get; set; }

			public void UpdateSinCos()
			{
				double a = Math.Atan((Point1.Y - Point2.Y) / (Point1.X - Point2.X));
				Sine = (float)Math.Sin(Math.Abs(a));
				Cosine = (float)Math.Cos(Math.Abs(a));
				if ((Point1.Y - Point2.Y) > 0) Sine = 0 - Sine;
				if ((Point1.X - Point2.X) < 0) Cosine = 0 - Cosine;
			}

			public void UpdateDistance(CPOIEntry Next)
			{
				if (GotoSection != -1)
				{
					Distance = -1;
					return;
				}
				UpdateSinCos();
				Next.UpdateSinCos();
				Distance =
					Next.Cosine * Next.Point2.Y + Next.Sine * Next.Point2.X + 0.5f -
					Cosine * Point1.Y - Sine * Point1.X + 0.5f;
			}
		}
	}
}
