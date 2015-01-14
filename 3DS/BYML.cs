using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;

namespace _3DS
{
	public class BYML : FileFormat<BYML.BYMLIdentifier>, IViewable
	{
		public BYML(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new BYMLHeader(er);
				er.BaseStream.Position = Header.NodeNameTableNodeOffset;
				NodeNameTableNode = new BYMLStringTableNode(er);
				er.BaseStream.Position = Header.StringValueTableNodeOffset;
				StringValueTableNode = new BYMLStringTableNode(er);
				er.BaseStream.Position = Header.RootNodeOffset;
				RootNode = BYMLFullNode.FromStream(er);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new Form();
		}

		public BYMLHeader Header;
		public class BYMLHeader
		{
			public BYMLHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 2);
				if (Signature != "YB") throw new SignatureNotCorrectException(Signature, "YB", er.BaseStream.Position - 2);
				Version = er.ReadUInt16();
				NodeNameTableNodeOffset = er.ReadUInt32();
				StringValueTableNodeOffset = er.ReadUInt32();
				RootNodeOffset = er.ReadUInt32();
			}
			public String Signature;
			public UInt16 Version;
			public UInt32 NodeNameTableNodeOffset;
			public UInt32 StringValueTableNodeOffset;
			public UInt32 RootNodeOffset;
		}

		public BYMLStringTableNode NodeNameTableNode;
		public BYMLStringTableNode StringValueTableNode;
		public BYMLFullNode RootNode;

		public class BYMLFullNode
		{
			public BYMLFullNode(EndianBinaryReader er)
			{
				NrEntries = er.ReadUInt32();
				NodeType = (byte)(NrEntries & 0xFF);
				NrEntries >>= 8;
			}
			public Byte NodeType;
			public UInt32 NrEntries;

			public static BYMLFullNode FromStream(EndianBinaryReader er)
			{
				byte type = er.ReadByte();
				er.BaseStream.Position--;
				switch (type)
				{
					case 0xC0: return new BYMLArrayNode(er);
					case 0xC1: return new BYMLDictNode(er);
				}
				return new BYMLFullNode(er);
			}

			public virtual void WriteYAML(BYML Owner, StringBuilder b, int Indent, bool FirstIndent = true)
			{
				if (FirstIndent) b.Append(new String(' ', Indent * 2));
				b.AppendLine("Unknown Full Node (0x" + NodeType.ToString("X2") + ")");
			}
		}

		public class BYMLArrayNode : BYMLFullNode
		{
			public BYMLArrayNode(EndianBinaryReader er)
				: base(er)
			{
				NodeTypes = er.ReadBytes((int)NrEntries);
				while ((er.BaseStream.Position % 4) != 0) er.ReadByte();
				Values = er.ReadUInt32s((int)NrEntries);
				FullNodes = new BYMLFullNode[NrEntries];
				for (int i = 0; i < NrEntries; i++)
				{
					if ((NodeTypes[i] >> 4) == 0xC)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Values[i];
						FullNodes[i] = BYMLFullNode.FromStream(er);
						er.BaseStream.Position = curpos;
					}
				}
			}
			public Byte[] NodeTypes;
			public UInt32[] Values;

			public BYMLFullNode[] FullNodes;

			public override void WriteYAML(BYML Owner, StringBuilder b, int Indent, bool FirstIndent = true)
			{
				for (int i = 0; i < NrEntries; i++)
				{
					if (i != 0 || FirstIndent) b.Append(new String(' ', Indent * 2));
					b.Append("- ");
					if ((NodeTypes[i] >> 4) == 0xC)
					{
						FullNodes[i].WriteYAML(Owner, b, Indent + 1, false);
						b.AppendLine();
					}
					else
					{
						FormatValueNode(Owner, b, NodeTypes[i], Values[i]);
						b.AppendLine();
					}
				}
			}
		}

		public class BYMLDictNode : BYMLFullNode
		{
			public BYMLDictNode(EndianBinaryReader er)
				: base(er)
			{
				SubNodes = new BYMLDictSubNode[NrEntries];
				for (int i = 0; i < NrEntries; i++)
				{
					SubNodes[i] = new BYMLDictSubNode(er);
				}
			}

			public BYMLDictSubNode[] SubNodes;
			public class BYMLDictSubNode
			{
				public BYMLDictSubNode(EndianBinaryReader er)
				{
					StringIndex = er.ReadUInt32();
					NodeType = (byte)(StringIndex >> 24);
					StringIndex &= 0xFFFFFF;
					Value = er.ReadUInt32();
					if ((NodeType >> 4) == 0xC)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Value;
						FullNode = BYMLFullNode.FromStream(er);
						er.BaseStream.Position = curpos;
					}
				}
				public UInt32 StringIndex;
				public Byte NodeType;
				public UInt32 Value;

				public BYMLFullNode FullNode;
			}

			public override void WriteYAML(BYML Owner, StringBuilder b, int Indent, bool FirstIndent = true)
			{
				for (int i = 0; i < NrEntries; i++)
				{
					if (i != 0 || FirstIndent) b.Append(new String(' ', Indent * 2));
					b.AppendFormat("{0}:", Owner.NodeNameTableNode.StringTable[SubNodes[i].StringIndex]);
					if ((SubNodes[i].NodeType >> 4) == 0xC)
					{
						b.AppendLine();
						SubNodes[i].FullNode.WriteYAML(Owner, b, Indent + 1);
					}
					else
					{
						FormatValueNode(Owner, b, SubNodes[i].NodeType, SubNodes[i].Value);
						b.AppendLine();
					}
				}
			}
		}

		private static void FormatValueNode(BYML Owner, StringBuilder b, byte Type, uint Value)
		{
			switch (Type)
			{
				case 0xA0: b.AppendFormat(" {0}", Owner.StringValueTableNode.StringTable[Value]); break;
				case 0xA1:
					break;
				case 0xD1: b.AppendFormat(" {0}", (int)Value); break;
				case 0xD2: b.AppendFormat(" {0}", BitConverter.ToSingle(BitConverter.GetBytes(Value), 0).ToString().Replace(",", ".")); break;
				default: b.AppendFormat(" {0:X8}", Value); break;
			}
		}

		public class BYMLStringTableNode : BYMLFullNode
		{
			public BYMLStringTableNode(EndianBinaryReader er)
				: base(er)
			{
				long basepos = er.BaseStream.Position - 4;
				StringOffsets = er.ReadUInt32s((int)NrEntries);
				StringTableEndOffset = er.ReadUInt32();
				long curpos = er.BaseStream.Position;
				StringTable = new string[NrEntries];
				for (int i = 0; i < NrEntries; i++)
				{
					er.BaseStream.Position = StringOffsets[i] + basepos;
					StringTable[i] = er.ReadStringNT(Encoding.ASCII);//.UTF8);
				}
				er.BaseStream.Position = curpos;
			}
			public UInt32[] StringOffsets;
			public UInt32 StringTableEndOffset;

			public String[] StringTable;
		}

		public String ToYAML()
		{
			StringBuilder b = new StringBuilder();
			RootNode.WriteYAML(this, b, 0);
			return b.ToString();
		}

		public class BYMLIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				//Tempoarly
				return "BYML Files";
			}

			public override string GetFileDescription()
			{
				return "Binary YAML File (BYML)";
			}

			public override string GetFileFilter()
			{
				return "Binary YAML File (*.byml)|*.byml";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 0x10 && File.Data[0] == 'Y' && File.Data[1] == 'B' && File.Data[2] == 1 && File.Data[3] == 0) return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
