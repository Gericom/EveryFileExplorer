using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;
using System.IO;
using LibEveryFileExplorer.Files;

namespace MarioKart.MK7.KMP
{
	public class POTI
	{
		public POTI()
		{
			Signature = "ITOP";
		}
		public POTI(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "ITOP") throw new SignatureNotCorrectException(Signature, "ITOP", er.BaseStream.Position - 4);
			NrRoutes = er.ReadUInt16();
			NrPoints = er.ReadUInt16();
			for (int i = 0; i < NrRoutes; i++) Routes.Add(new POTIRoute(er));
		}
		public String Signature;
		public UInt16 NrRoutes;
		public UInt16 NrPoints;

		public List<POTIRoute> Routes = new List<POTIRoute>();
		public class POTIRoute
		{
			public POTIRoute(EndianBinaryReader er)
			{
				NrPoints = er.ReadUInt16();
				Setting1 = er.ReadByte();
				Setting2 = er.ReadByte();
				for (int i = 0; i < NrPoints; i++) Points.Add(new POTIPoint(er));
			}
			public UInt16 NrPoints;
			public Byte Setting1;
			public Byte Setting2;

			public List<POTIPoint> Points = new List<POTIPoint>();
			public class POTIPoint
			{
				public POTIPoint(EndianBinaryReader er)
				{
					Position = er.ReadVector3();
					Setting1 = er.ReadUInt16();
					Setting2 = er.ReadUInt16();
				}
				public Vector3 Position;
				public UInt16 Setting1;
				public UInt16 Setting2;
			}
		}
	}
}
