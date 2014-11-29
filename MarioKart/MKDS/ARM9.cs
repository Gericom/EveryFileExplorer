using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarioKart.MKDS
{
	public class ARM9
	{
		public static readonly ARM9Table[] Tables =
		{
			new ARM9Table("Local Map Scale Table", 0x0216875C, 0, 55, 
				new Field("MinX", typeof(Int16)),
				new Field("MinY", typeof(Int16)),
				new Field("MaxX", typeof(Int16)),
				new Field("MaxY", typeof(Int16))),
			new ARM9Table("Global Map Scale Table", 0x021685A4, 0, 55,
				new Field("MinX", typeof(Int16)),
				new Field("MinY", typeof(Int16)),
				new Field("MaxX", typeof(Int16)),
				new Field("MaxY", typeof(Int16)))
		};

		public static readonly ARM9Field[] Fields =
		{
			new ARM9Field("Beach Water Tide Speed", typeof(UInt32), 0x020D9C38, 0),
			new ARM9Field("Beach Water Up Tide", typeof(Single), 0x020D9C3C, 0),
			new ARM9Field("Beach Water Down Tide", typeof(Single), 0x020D9C40, 0),
		};

		public class ARM9Table
		{
			public ARM9Table(String Name, UInt32 EUAddress, UInt32 USAddress, int NrEntries, params Field[] Fields)
			{
				this.Name = Name;
				this.EUAddress = EUAddress;
				this.USAddress = USAddress;
				this.NrEntries = NrEntries;
				this.Fields = Fields;
			}
			public String Name { get; private set; }
			public UInt32 EUAddress { get; private set; }
			public UInt32 USAddress { get; private set; }
			public int NrEntries { get; private set; }
			public Field[] Fields { get; private set; }
		}

		public class Field
		{
			public Field(String Name, Type Type)
			{
				if (!Type.IsPrimitive) throw new ArgumentException();
				this.Name = Name;
				this.Type = Type;
			}

			public object Read(NDS.Nitro.ARM9 Code, ref UInt32 Address)
			{
				object tmp;
				switch (Type.Name)
				{
					case "Boolean":
					case "Byte":
					case "SByte":
					case "Int16":
					case "UInt16":
					case "Int32":
						goto default;
					case "UInt32": tmp = Code.ReadU32LE(Address); Address += 4; return tmp;
					case "Int64":
					case "UInt64":
					case "IntPtr":
					case "UIntPtr":
					case "Char":
					case "Double":
						goto default;
					//read as Fx32
					case "Single": tmp = (float)Code.ReadU32LE(Address) / 4096f; Address += 4; return tmp;
					default:
						throw new Exception(Type.Name + " is not supported!");
				}
			}

			public String Name { get; private set; }
			public Type Type { get; private set; }
		}

		public class ARM9Field : Field
		{
			public ARM9Field(String Name, Type Type, UInt32 EUAddress, UInt32 USAddress)
				: base(Name, Type)
			{
				this.EUAddress = EUAddress;
				this.USAddress = USAddress;
			}

			public UInt32 EUAddress {get; private set;}
			public UInt32 USAddress {get; private set;}
		}
	}
}
