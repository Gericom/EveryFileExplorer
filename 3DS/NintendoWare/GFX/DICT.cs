using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;

namespace _3DS.NintendoWare.GFX
{
	public class DICT
	{
		public DICT()
		{
			Signature = "DICT";
			RootNode = new Node();
			Entries = new List<Node>();
		}
		public DICT(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "DICT") throw new SignatureNotCorrectException(Signature, "DICT", er.BaseStream.Position);
			SectionSize = er.ReadUInt32();
			NrEntries = er.ReadUInt32();

			RootNode = new Node(er);
			Entries = new List<Node>();// new Node[NrEntries];
			for (int i = 0; i < NrEntries; i++)
			{
				Entries.Add(new Node(er));
			}
		}

		public void Write(EndianBinaryWriter er, CGFXWriterContext c)
		{
			long basepos = er.BaseStream.Position;
			er.Write(Signature, Encoding.ASCII, false);
			er.Write((uint)0);
			er.Write((uint)Entries.Count);
			RootNode.Write(er, c);
			foreach (var v in Entries) v.Write(er, c);
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = basepos + 4;
			er.Write((uint)(curpos - basepos));
			er.BaseStream.Position = curpos;
		}

		private String Signature;
		private UInt32 SectionSize;
		private UInt32 NrEntries;

		private Node RootNode;
		private List<Node> Entries;
		public class Node
		{
			public Node() { }
			public Node(EndianBinaryReader er)
			{
				RefBit = er.ReadUInt32();
				LeftIndex = er.ReadUInt16();
				RightIndex = er.ReadUInt16();
				NameOffset = er.ReadUInt32();
				if (NameOffset != 0) NameOffset += (UInt32)er.BaseStream.Position - 4;
				DataOffset = er.ReadUInt32();
				if (DataOffset != 0) DataOffset += (UInt32)er.BaseStream.Position - 4;

				if (NameOffset != 0)
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = NameOffset;
					Name = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
			}

			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				er.Write(RefBit);
				er.Write(LeftIndex);
				er.Write(RightIndex);
				if (Name != null) c.WriteStringReference(Name, er);
				else er.Write((uint)0);
				er.Write((uint)0);
			}

			public UInt32 RefBit;
			public UInt16 LeftIndex;
			public UInt16 RightIndex;
			public UInt32 NameOffset;
			public UInt32 DataOffset;

			public String Name;
			public override string ToString()
			{
				return Name;
			}
		}

		public int Count { get { return Entries.Count; } }

		public int IndexOf(uint Offset)
		{
			for (int i = 0; i < NrEntries; i++)
			{
				if (Entries[i].DataOffset == Offset) return i;
			}
			return -1;
		}

		public int IndexOf(String Name)
		{
			for (int i = 0; i < NrEntries; i++)
			{
				if (Entries[i].Name == Name) return i;
			}
			return -1;
		}

		public Node this[int index]
		{
			get { return Entries[index]; }
			set { Entries[index] = value; RegenerateTree(); }
		}

		public Node this[String index]
		{
			get { return Entries[IndexOf(index)]; }
			set { Entries[IndexOf(index)] = value; RegenerateTree(); }
		}

		public void Add(String Name)
		{
			Entries.Add(new Node() { Name = Name });
			NrEntries = (uint)Entries.Count;
			RegenerateTree();
		}

		public void Remove(Node Entry)
		{
			Entries.Remove(Entry);
			NrEntries = (uint)Entries.Count;
			RegenerateTree();
		}

		public void RemoveAt(int Index)
		{
			Entries.RemoveAt(Index);
			RegenerateTree();
		}

		private void RegenerateTree()
		{
			List<String> names = new List<string>();
			foreach (Node n in Entries)
			{
				names.Add(n.Name);
			}
			PatriciaTreeGenerator p = PatriciaTreeGenerator.Generate(names.ToArray());
			bool first = true;
			foreach (var v in p.TreeNodes)
			{
				if (first)
				{
					v.idxEntry = -1;
					RootNode.LeftIndex = (ushort)(v.left.idxEntry + 1);
					RootNode.RightIndex = (ushort)(v.right.idxEntry + 1);
					RootNode.RefBit = 0xFFFFFFFF;
					first = false;
					continue;
				}
				Entries[v.idxEntry].LeftIndex = (ushort)(v.left.idxEntry + 1);
				Entries[v.idxEntry].RightIndex = (ushort)(v.right.idxEntry + 1);
				Entries[v.idxEntry].RefBit = (uint)v.refbit;
			}
		}
	}
}
