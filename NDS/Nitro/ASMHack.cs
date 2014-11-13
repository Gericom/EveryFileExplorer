using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace NDS.Nitro
{
	public class ASMHack
	{
		public void Insert(String Path, ARM9 Arm9)
		{
			ASMHackInfo Info = ASMHackInfo.FromByteArray(File.ReadAllBytes(Path + "\\config.xml"));

			UInt32 ArenaLoOffset = Info.ArenaLo;//UInt32.Parse(File.ReadAllText(CodeDir + "\\arenaoffs.txt"), System.Globalization.NumberStyles.HexNumber);
			UInt32 ArenaLo = Arm9.ReadU32LE(ArenaLoOffset);
			Compile(Path, ArenaLo);
			if (!File.Exists(Path + "\\newcode.bin"))
			{
				MessageBox.Show("An compile error occurred!");
				return;
			}
			byte[] NewCode = File.ReadAllBytes(Path + "\\newcode.bin");

			StreamReader r = new StreamReader(Path + "\\newcode.sym");
			string CurrentLine;
			while ((CurrentLine = r.ReadLine()) != null)
			{
				string[] Line = CurrentLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (Line.Length == 4)
				{
					if (Line[3].Length < 7) continue;
					switch (Line[3].Remove(6))
					{
						case "arepl_":
							{
								String ReplaceOffsetString = Line[3].Replace("arepl_", "");
								UInt32 ReplaceOffset = UInt32.Parse(ReplaceOffsetString, System.Globalization.NumberStyles.HexNumber);
								UInt32 Replace = 0xEB000000;//BL Instruction
								UInt32 DestinationOffset = UInt32.Parse(Line[0], System.Globalization.NumberStyles.HexNumber);
								UInt32 RelativeDestinationOffset = (DestinationOffset / 4) - (ReplaceOffset / 4) - 2;
								RelativeDestinationOffset &= 0x00FFFFFF;
								Replace |= RelativeDestinationOffset;
								if (!Arm9.WriteU32LE(ReplaceOffset, Replace)) System.Windows.Forms.MessageBox.Show("The offset of function " + Line[3] + " is invalid. Maybe your code is inside an overlay or you wrote the wrong offset.");
								break;
							}
						case "ansub_":
							{
								String ReplaceOffsetString = Line[3].Replace("ansub_", "");
								UInt32 ReplaceOffset = UInt32.Parse(ReplaceOffsetString, System.Globalization.NumberStyles.HexNumber);
								UInt32 Replace = 0xEA000000;//B Instruction
								UInt32 DestinationOffset = UInt32.Parse(Line[0], System.Globalization.NumberStyles.HexNumber);
								UInt32 RelativeDestinationOffset = (DestinationOffset / 4) - (ReplaceOffset / 4) - 2;
								RelativeDestinationOffset &= 0x00FFFFFF;
								Replace |= RelativeDestinationOffset;
								if (!Arm9.WriteU32LE(ReplaceOffset, Replace)) System.Windows.Forms.MessageBox.Show("The offset of function " + Line[3] + " is invalid. Maybe your code is inside an overlay or you wrote the wrong offset.");
								break;
							}
						case "trepl_":
							{
								String ReplaceOffsetString = Line[3].Replace("trepl_", "");
								UInt32 ReplaceOffset = UInt32.Parse(ReplaceOffsetString, System.Globalization.NumberStyles.HexNumber);
								UInt16 Replace1 = 0xF000;//BLX Instruction (Part 1)
								UInt16 Replace2 = 0xE800;//BLX Instruction (Part 2)
								UInt32 DestinationOffset = UInt32.Parse(Line[0], System.Globalization.NumberStyles.HexNumber);
								UInt32 RelativeDestinationOffset = DestinationOffset - ReplaceOffset - 2;
								RelativeDestinationOffset >>= 1;
								RelativeDestinationOffset &= 0x003FFFFF;
								Replace1 |= (UInt16)((RelativeDestinationOffset >> 11) & 0x7FF);
								Replace2 |= (UInt16)((RelativeDestinationOffset >> 0) & 0x7FE);
								if (!Arm9.WriteU16LE(ReplaceOffset, Replace1)) { MessageBox.Show("The offset of function " + Line[3] + " is invalid. Maybe your code is inside an overlay or you wrote the wrong offset.\r\nIf your code is inside an overlay, this is an action replay code to let your asm hack still work:\r\n1" + string.Format("{0:X7}", ReplaceOffset) + " 0000" + string.Format("{0:X4}", Replace1) + "\r\n1" + string.Format("{0:X7}", ReplaceOffset + 2) + " 0000" + string.Format("{0:X4}", Replace2)); break; }
								else Arm9.WriteU16LE(ReplaceOffset + 2, Replace2);
								break;
							}
					}
				}
			}
			r.Close();
			Arm9.WriteU32LE(ArenaLoOffset, ArenaLo + (uint)NewCode.Length);
			Arm9.AddAutoLoadEntry(ArenaLo, NewCode);
			File.Delete(Path + "\\newcode.bin");
			File.Delete(Path + "\\newcode.elf");
			File.Delete(Path + "\\newcode.sym");
			Directory.Delete(Path + "\\build", true);
		}

		private static void Compile(String Path, UInt32 ArenaLo)
		{
			Process p = new Process();
			p.StartInfo.FileName = "cmd";
			p.StartInfo.Arguments = "/C make CODEADDR=0x" + ArenaLo.ToString("X8") + " || pause";
			p.StartInfo.WorkingDirectory = Path;
			p.Start();
			p.WaitForExit();
		}

		[Serializable]
		[XmlRoot("ASMHack")]
		public class ASMHackInfo : IXmlSerializable
		{
			public ASMHackInfo() { }
			public ASMHackInfo(NDS Rom, UInt32 AreaLo)
			{
				RamAddress = Rom.Header.MainRamAddress;
				this.ArenaLo = AreaLo;
			}

			public UInt32 RamAddress;
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
				writer.WriteStartElement("ArenaLo");
				{
					writer.WriteString("0x" + ArenaLo.ToString("X8"));
				}
				writer.WriteEndElement();
			}

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
}
