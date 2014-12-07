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
	public class CAME : GameDataSection<CAME.CAMEEntry>
	{
		public CAME() { Signature = "EMAC"; }
		public CAME(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "EMAC") throw new SignatureNotCorrectException(Signature, "EMAC", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt16();
			Unknown = er.ReadUInt16();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new CAMEEntry(er));
		}
		public UInt16 Unknown;

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"Type",
					"Next",
					"?",
					"Route ID",
					"Route Speed",
					"FOV Speed",
					"Viewpoint Speed",
					"?",
					"?",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"VOF Begin",
					"VOF End",
					"VP 1 X", "VP 1 Y", "VP 1 Z",
					"VP 2 X", "VP 2 Y", "VP 2 Z",
					"Duration"
				};
		}
		public class CAMEEntry : GameDataSectionEntry
		{
			public CAMEEntry()
			{
				
			}
			public CAMEEntry(EndianBinaryReader er)
			{
				Type = er.ReadByte();
				Next = er.ReadByte();
				Unknown1 = er.ReadByte();
				RouteID = er.ReadByte();
				RouteSpeed = er.ReadUInt16();
				FOVSpeed = er.ReadUInt16();
				ViewpointSpeed = er.ReadUInt16();
				Unknown2 = er.ReadByte();
				Unknown3 = er.ReadByte();
				Position = er.ReadVector3();
				Rotation = er.ReadVector3();
				FOVBegin = er.ReadSingle();
				FOVEnd = er.ReadSingle();
				Viewpoint1 = er.ReadVector3();
				Viewpoint2 = er.ReadVector3();
				Duration = er.ReadSingle();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Type.ToString());
				m.SubItems.Add(Next.ToString());
				m.SubItems.Add(Unknown1.ToString());
				m.SubItems.Add(RouteID.ToString());

				m.SubItems.Add(RouteSpeed.ToString());
				m.SubItems.Add(FOVSpeed.ToString());
				m.SubItems.Add(ViewpointSpeed.ToString());

				m.SubItems.Add(Unknown2.ToString());
				m.SubItems.Add(Unknown3.ToString());

				m.SubItems.Add(Position.X.ToString());
				m.SubItems.Add(Position.Y.ToString());
				m.SubItems.Add(Position.Z.ToString());

				m.SubItems.Add(Rotation.X.ToString());
				m.SubItems.Add(Rotation.Y.ToString());
				m.SubItems.Add(Rotation.Z.ToString());

				m.SubItems.Add(FOVBegin.ToString());
				m.SubItems.Add(FOVEnd.ToString());

				m.SubItems.Add(Viewpoint1.X.ToString());
				m.SubItems.Add(Viewpoint1.Y.ToString());
				m.SubItems.Add(Viewpoint1.Z.ToString());

				m.SubItems.Add(Viewpoint2.X.ToString());
				m.SubItems.Add(Viewpoint2.Y.ToString());
				m.SubItems.Add(Viewpoint2.Z.ToString());

				m.SubItems.Add(Duration.ToString());
				return m;
			}
			public Byte Type { get; set; }
			public Byte Next { get; set; }
			public Byte Unknown1 { get; set; }
			public Byte RouteID { get; set; }
			public UInt16 RouteSpeed { get; set; }
			public UInt16 FOVSpeed { get; set; }
			public UInt16 ViewpointSpeed { get; set; }
			public Byte Unknown2 { get; set; }
			public Byte Unknown3 { get; set; }
			public Vector3 Position { get; set; }
			public Vector3 Rotation { get; set; }
			public Single FOVBegin { get; set; }
			public Single FOVEnd { get; set; }
			public Vector3 Viewpoint1 { get; set; }
			public Vector3 Viewpoint2 { get; set; }
			public Single Duration { get; set; }
		}
	}
}
