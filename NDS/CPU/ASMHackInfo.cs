using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace NDS.CPU
{
	[Serializable]
	[XmlRoot("ASMHack")]
	public class ASMHackInfo
	{
		public ASMHackInfo() { }
		public ASMHackInfo(NDS Rom, UInt32 AreaLo)
		{
			AddressInfo.RamAddress = Rom.Header.MainRamAddress;
			AddressInfo.EntryAddress = Rom.Header.MainEntryAddress;
			AddressInfo.AutoloadDoneAddress = Rom.Header.MainAutoloadDone;
			AddressInfo.ArenaLo = AreaLo;
			//Default:
			InPath = "arm9.bin";
			OutPath = "arm9_new.bin";
		}

		[XmlElement("Addresses")]
		public Addresses AddressInfo = new Addresses();
		public class Addresses : IXmlSerializable
		{
			public UInt32 RamAddress;
			public UInt32 EntryAddress;
			public UInt32 AutoloadDoneAddress;
			public UInt32 ArenaLo;

			public System.Xml.Schema.XmlSchema GetSchema()
			{
				return null;
			}

			public void ReadXml(System.Xml.XmlReader reader)
			{
				reader.ReadStartElement("RamAddress");
				{
					RamAddress = uint.Parse(reader.ReadString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
				}
				reader.ReadEndElement();
				reader.ReadStartElement("EntryAddress");
				{
					EntryAddress = uint.Parse(reader.ReadString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
				}
				reader.ReadEndElement();
				reader.ReadStartElement("AutoloadDoneAddress");
				{
					AutoloadDoneAddress = uint.Parse(reader.ReadString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
				}
				reader.ReadEndElement();
				reader.ReadStartElement("ArenaLo");
				{
					ArenaLo = uint.Parse(reader.ReadString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
				}
				reader.ReadEndElement();
			}

			public void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("RamAddress");
				{
					writer.WriteString("0x" + RamAddress.ToString("X8"));
				}
				writer.WriteEndElement();
				writer.WriteStartElement("EntryAddress");
				{
					writer.WriteString("0x" + EntryAddress.ToString("X8"));
				}
				writer.WriteEndElement();
				writer.WriteStartElement("AutoloadDoneAddress");
				{
					writer.WriteString("0x" + AutoloadDoneAddress.ToString("X8"));
				}
				writer.WriteEndElement();
				writer.WriteStartElement("ArenaLo");
				{
					writer.WriteString("0x" + ArenaLo.ToString("X8"));
				}
				writer.WriteEndElement();
			}
		}
		public String InPath;
		public String OutPath;

		public static ASMHackInfo FromByteArray(byte[] Data)
		{
			XmlSerializer s = new XmlSerializer(typeof(ASMHackInfo));
			return (ASMHackInfo)s.Deserialize(new MemoryStream(Data));
		}

		public byte[] Write()
		{
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("", "");
			XmlSerializer s = new XmlSerializer(typeof(ASMHackInfo));
			MemoryStream m = new MemoryStream();
			s.Serialize(m, this, ns);
			byte[] data = m.ToArray();
			m.Close();
			return data;
		}
	}
}
