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
	public class CAME : GameDataSection<CAME.CAMEEntry>
	{
		public CAME() { Signature = "CAME"; }
		public CAME(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "CAME") throw new SignatureNotCorrectException(Signature, "CAME", er.BaseStream.Position - 4);
			NrEntries = er.ReadUInt32();
			for (int i = 0; i < NrEntries; i++) Entries.Add(new CAMEEntry(er));
		}

		public override String[] GetColumnNames()
		{
			return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"VP1 X", "VP1 Y", "VP1 Z",
					"VP2 X", "VP2 Y", "VP2 Z",
					"Fov Begin",
					"Fov Sin",
					"Fov Cos",
					"Fov End",
					"Fov Sin",
					"Fov Cos",
					"Fov Speed",
					"Cam Type",
					"Route ID",
					"Route Speed",
					"Point Speed",
					"Duration",
					"Next Cam",
					"1st Intro",
					"?"
				};
		}

		public class CAMEEntry : GameDataSectionEntry
		{
			public enum CAMEIntroCamera
			{
				No = 0,
				Top = 1,
				Bottom = 2
			}

			public CAMEEntry()
			{
				FieldOfViewBegin = 30;
				FieldOfViewEnd = 30;
				LinkedRoute = -1;
				UpdateSinCos();
			}
			public CAMEEntry(EndianBinaryReader er)
			{
				Position = er.ReadVecFx32();
				Angle = er.ReadVecFx32();
				Viewpoint1 = er.ReadVecFx32();
				Viewpoint2 = er.ReadVecFx32();
				FieldOfViewBegin = er.ReadUInt16();
				FieldOfViewBeginSine = er.ReadFx16();
				FieldOfViewBeginCosine = er.ReadFx16();
				FieldOfViewEnd = er.ReadUInt16();
				FieldOfViewEndSine = er.ReadFx16();
				FieldOfViewEndCosine = er.ReadFx16();
				FovSpeed = er.ReadInt16();
				CameraType = er.ReadInt16();
				LinkedRoute = er.ReadInt16();
				RouteSpeed = er.ReadInt16();
				PointSpeed = er.ReadInt16();
				Duration = er.ReadInt16();
				NextCamera = er.ReadInt16();
				FirstIntroCamera = (CAMEIntroCamera)er.ReadByte();
				Unknown5 = er.ReadByte();
			}

			public override ListViewItem GetListViewItem()
			{
				ListViewItem m = new ListViewItem("");
				m.SubItems.Add(Position.X.ToString("#####0.############"));
				m.SubItems.Add(Position.Y.ToString("#####0.############"));
				m.SubItems.Add(Position.Z.ToString("#####0.############"));

				m.SubItems.Add(Angle.X.ToString("#####0.############"));
				m.SubItems.Add(Angle.Y.ToString("#####0.############"));
				m.SubItems.Add(Angle.Z.ToString("#####0.############"));

				m.SubItems.Add(Viewpoint1.X.ToString("#####0.############"));
				m.SubItems.Add(Viewpoint1.Y.ToString("#####0.############"));
				m.SubItems.Add(Viewpoint1.Z.ToString("#####0.############"));

				m.SubItems.Add(Viewpoint2.X.ToString("#####0.############"));
				m.SubItems.Add(Viewpoint2.Y.ToString("#####0.############"));
				m.SubItems.Add(Viewpoint2.Z.ToString("#####0.############"));

				m.SubItems.Add(FieldOfViewBegin.ToString());
				m.SubItems.Add(FieldOfViewBeginSine.ToString("#####0.############"));
				m.SubItems.Add(FieldOfViewBeginCosine.ToString("#####0.############"));

				m.SubItems.Add(FieldOfViewEnd.ToString());
				m.SubItems.Add(FieldOfViewEndSine.ToString("#####0.############"));
				m.SubItems.Add(FieldOfViewEndCosine.ToString("#####0.############"));

				m.SubItems.Add(FovSpeed.ToString());
				m.SubItems.Add(CameraType.ToString());

				m.SubItems.Add(LinkedRoute.ToString());

				m.SubItems.Add(RouteSpeed.ToString());
				m.SubItems.Add(PointSpeed.ToString());

				m.SubItems.Add(Duration.ToString());

				m.SubItems.Add(NextCamera.ToString());

				m.SubItems.Add(FirstIntroCamera.ToString());
				m.SubItems.Add(HexUtil.GetHexReverse(Unknown5));
				return m;
			}

			[Category("Transformation")]
			public Vector3 Position { get; set; }
			[Category("Transformation")]
			public Vector3 Angle { get; set; }
			[Category("Viewpoints"), DisplayName("Viewpoint 1")]
			public Vector3 Viewpoint1 { get; set; }
			[Category("Viewpoints"), DisplayName("Viewpoint 2")]
			public Vector3 Viewpoint2 { get; set; }
			[Category("Field of View"), DisplayName("Begin Angle")]
			public UInt16 FieldOfViewBegin { get; set; }
			[Browsable(false)]
			public Single FieldOfViewBeginSine { get; private set; }//2
			[Browsable(false)]
			public Single FieldOfViewBeginCosine { get; private set; }//2
			[Category("Field of View"), DisplayName("End Angle")]
			public UInt16 FieldOfViewEnd { get; set; }
			[Browsable(false)]
			public Single FieldOfViewEndSine { get; private set; }
			[Browsable(false)]
			public Single FieldOfViewEndCosine { get; private set; }
			[Category("Field of View"), DisplayName("Speed")]
			public Int16 FovSpeed { get; set; }
			[Category("Camera"), DisplayName("Type")]
			public Int16 CameraType { get; set; }
			[Category("Camera"), DisplayName("Linked Route")]
			public Int16 LinkedRoute { get; set; }
			[Category("Camera"), DisplayName("Route Speed")]
			public Int16 RouteSpeed { get; set; }
			[Category("Viewpoints"), DisplayName("Speed")]
			public Int16 PointSpeed { get; set; }
			[Category("Camera")]
			public Int16 Duration { get; set; }
			[Category("Camera"), DisplayName("Next Camera")]
			public Int16 NextCamera { get; set; }
			[Category("Camera"), DisplayName("First Intro Camera")]
			[Description("Specifies if this CAME is the first camera to use for the course intro for the top or bottom screen.")]
			public CAMEIntroCamera FirstIntroCamera { get; set; }//byte
			[Category("Camera")]
			public Byte Unknown5 { get; set; }

			public void UpdateSinCos()
			{
				FieldOfViewBeginSine = (float)Math.Sin(MathUtil.DegToRad(FieldOfViewBegin));
				FieldOfViewBeginCosine = (float)Math.Cos(MathUtil.DegToRad(FieldOfViewBegin));
				FieldOfViewEndSine = (float)Math.Sin(MathUtil.DegToRad(FieldOfViewEnd));
				FieldOfViewEndCosine = (float)Math.Cos(MathUtil.DegToRad(FieldOfViewEnd));
			}
		}
	}
}
