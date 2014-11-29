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
using LibEveryFileExplorer.Math;

namespace MarioKart.MKDS.NKM
{
	public class KTPJ : GameDataSection<KTPJ.KTPJEntry>
	{
		public KTPJ() { Signature = "KTPJ"; }
		public KTPJ(EndianBinaryReader er, UInt16 Version)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "KTPJ") throw new SignatureNotCorrectException(Signature, "KTPJ", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPJEntry(er, Version));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"EPOI Id",
					"IPOI Id",
					"Index"
				};
		}

		public class KTPJEntry : GameDataSectionEntry
		{
			private UInt16 Version = 37;
			public KTPJEntry()
			{
				Position = new Vector3(0, 0, 0);
				Rotation = new Vector3(0, 0, 0);
				EnemyPointID = 0;
				ItemPointID = 0;
				Index = 0;
			}
			public KTPJEntry(EndianBinaryReader er, UInt16 Version)
			{
				this.Version = Version;
				Position = er.ReadVecFx32();
				Rotation = er.ReadVecFx32();
				if (Version <= 34)
				{
					float yangle = (float)Math.Atan2(Rotation.X, Rotation.Z);
					Rotation = new Vector3(0, MathUtil.RadToDeg(yangle), 0);
				}
				EnemyPointID = er.ReadInt16();
				ItemPointID = er.ReadInt16();
				if (Version >= 34) Index = er.ReadInt32();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(Rotation.X.ToString("#####0.############"));
				m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
				m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

				m.SubItems.Add(EnemyPointID.ToString());
				m.SubItems.Add(ItemPointID.ToString());
				m.SubItems.Add(Index.ToString());
				return m;
			}
			[Category("Transformation")]
			public Vector3 Position { get; set; }
			[Category("Transformation")]
			public Vector3 Rotation { get; set; }
			[Category("Respawn"), DisplayName("Enemy Point ID")]
			public Int16 EnemyPointID { get; set; }
			[Category("Respawn"), DisplayName("Item Point ID")]
			public Int16 ItemPointID { get; set; }
			[Category("Respawn")]
			public Int32 Index { get; set; }
		}
	}
}
