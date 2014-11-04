using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using DictName = System.String;

namespace NDS.NitroSystem.G3D
{
	public class Dictionary<T> where T : DictionaryData, new()
	{
		public Dictionary(EndianBinaryReader er)
		{
			revision = er.ReadByte();
			numEntry = er.ReadByte();
			sizeDictBlk = er.ReadUInt16();
			er.ReadBytes(2);//PADDING(2 bytes);
			ofsEntry = er.ReadUInt16();
			node = new PtreeNode[numEntry + 1];
			for (int i = 0; i < numEntry + 1; i++)
			{
				node[i] = new PtreeNode();
				node[i].refBit = er.ReadByte();
				node[i].idxLeft = er.ReadByte();
				node[i].idxRight = er.ReadByte();
				node[i].idxEntry = er.ReadByte();
			}
			entry = new DictEntry();
			entry.sizeUnit = er.ReadUInt16();
			entry.ofsName = er.ReadUInt16();
			entry.data = new List<T>();//[numEntry];
			for (int i = 0; i < numEntry; i++)
			{
				entry.data.Add(new T());
				//entry.data[i] = new T();
				entry.data[i].Read(er);
			}
			names = new List<DictName>();//[numEntry];
			for (int i = 0; i < numEntry; i++)
			{
				//names[i] = new DictName();
				names.Add(er.ReadString(ASCIIEncoding.ASCII, 0x10).Replace("\0", ""));
			}
		}
		public Dictionary()
		{
			entry = new DictEntry();
			names = new List<DictName>();
			RegenerateTree();
		}
		public void Write(EndianBinaryWriter er)
		{
			long startpos = er.BaseStream.Position;
			er.Write(revision);
			er.Write(numEntry);
			er.Write((UInt16)0);
			er.Write((UInt16)8);
			er.Write((UInt16)((numEntry + 1) * 4 + 8));
			foreach (PtreeNode n in node) n.Write(er);
			entry.Write(er);
			foreach (DictName n in names) er.Write(n.PadRight(16, '\0'), ASCIIEncoding.ASCII, false);
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = startpos + 2;
			er.Write((UInt16)(curpos - startpos));
			er.BaseStream.Position = curpos;
		}
		public bool Contains(String s)
		{
			return names.Contains(s);
		}
		public bool Contains(T d)
		{
			return entry.data.Contains(d);
		}
		public int IndexOf(String s)
		{
			if (numEntry < 16)
			{
				int idx = 0;
				foreach (String ss in names)
				{
					if (ss == s) return idx;
					idx++;
				}
			}
			else
			{
				s.PadRight(16, '\0');
				int treeBase;
				PtreeNode p, x;

				treeBase = 0;
				p = node[0];

				if (p.idxLeft != 0)
				{
					x = node[treeBase + p.idxLeft];
					while (p.refBit > x.refBit)
					{
						p = x;

						uint val = 0;
						int idx = (x.refBit >> 5) * 4;
						int bitidx = 3;
						while (idx < s.Length)
						{
							val |= (uint)(s[idx] << (bitidx * 8));
							idx++;
							bitidx--;
						}


						x = node[treeBase /*+ x.idxLeft + )*/ + ((s[x.refBit / 8] >> (x.refBit - ((x.refBit / 8) * 8)) & 1) == 0 ? x.idxLeft : x.idxRight)];
					}

					String Name = names[x.idxEntry];
					Name.PadRight(16, '\0');
					//n = NNS_G3dGetResNameByIdx(dict, x->idxEntry);

					if (Name == s)
					{
						return x.idxEntry;
					}
				}
			}
			return -1;
		}
		public void Add(String Name, T Data)
		{
			entry.data.Add(Data);
			names.Add(Name);
			numEntry++;
			RegenerateTree();
		}
		public void Insert(int Index, String Name, T Data)
		{
			entry.data.Insert(Index, Data);
			names.Insert(Index, Name);
			numEntry++;
			RegenerateTree();
		}
		public void RemoveAt(int Index)
		{
			names.RemoveAt(Index);
			entry.data.RemoveAt(Index);
			numEntry--;
			RegenerateTree();
		}
		private void RegenerateTree()
		{
			node = PatriciaTreeGenerator.Generate(names.ToArray());
		}

		private class PatriciaTreeGenerator
		{
			List<PatTreeNode> Nodes = new List<PatTreeNode>();
			public PatriciaTreeGenerator()
			{
				AddRootPatTreeNode();
			}
			private PatTreeNode AddRootPatTreeNode()
			{
				PatTreeNode p = new PatTreeNode();
				p.refbit = 127;
				p.left = p;
				p.right = p;
				p.idxEntry = 0;
				p.name = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
				Nodes.Add(p);
				return p;
			}
			public PatTreeNode AddPatTreeNode(String Name, int Index)
			{
				foreach (PatTreeNode n in Nodes) if (n.name == Name) return null;
				PatTreeNode p = new PatTreeNode();
				Name = Name.PadRight(16, '\0');

				int CurRefBit = 0;
				PatTreeNode LastNode = null;

				PatTreeNode CurNode = Nodes[0].left;
				if (Nodes[0].refbit > Nodes[0].left.refbit)
				{
					CurRefBit = Nodes[0].left.refbit;
					do
					{
						LastNode = CurNode;
						if (((GetStringPart(Name, ((CurRefBit >> 5) & 3) * 4) >> (CurRefBit & 0x1F)) & 1) != 0) CurNode = CurNode.right;
						else CurNode = CurNode.left;
						CurRefBit = CurNode.refbit;
					}
					while (LastNode.refbit > CurRefBit);
				}

				CurRefBit = 0;
				LastNode = null;

				int Refbit = 127;
				if ((CurNode.idxEntry ^ GetStringPart(Name, 0xC)) >= 0)
				{
					int WorkingRefBit;
					do
					{
						Refbit--;
						WorkingRefBit = Refbit;
						WorkingRefBit >>= 5;
						WorkingRefBit &= 3;
						int c = GetStringPart(CurNode.name, WorkingRefBit * 4);
						c ^= GetStringPart(Name, WorkingRefBit * 4);
						int TmpRefBit = Refbit;
						TmpRefBit &= 0x1F;
						WorkingRefBit = c;
						WorkingRefBit >>= TmpRefBit & 0xFF;
					}
					while ((WorkingRefBit & 0x1) == 0);
				}

				CurRefBit = 0;
				LastNode = null;

				CurNode = Nodes[0].left;
				LastNode = Nodes[0];
				CurRefBit = Nodes[0].left.refbit;
				if (Nodes[0].refbit > Nodes[0].left.refbit)
				{
					do
					{
						if (CurRefBit <= Refbit) break;
						LastNode = CurNode;
						if (((GetStringPart(Name, ((CurRefBit >> 5) & 3) * 4) >> (CurRefBit & 0x1F)) & 1) != 0) CurNode = CurNode.right;
						else CurNode = CurNode.left;
						CurRefBit = CurNode.refbit;
					}
					while (LastNode.refbit > CurRefBit);
				}

				p.refbit = Refbit;
				p.left = null;//Nodes[0];
				p.right = null;//p;
				p.idxEntry = Index;
				p.name = Name;

				if (((GetStringPart(Name, ((Refbit >> 5) & 3) * 4) >> (Refbit & 0x1F)) & 1) == 0) p.left = p;
				else p.left = CurNode;

				if (((GetStringPart(Name, ((Refbit >> 5) & 3) * 4) >> (Refbit & 0x1F)) & 1) == 0) p.right = CurNode;
				else p.right = p;

				if (((GetStringPart(Name, ((LastNode.refbit >> 5) & 3) * 4) >> (LastNode.refbit & 0x1F)) & 1) != 0)
				{
					LastNode.right = p;
					Nodes.Add(p);
					return p;
				}
				else
				{
					LastNode.left = p;
					Nodes.Add(p);
					return p;
				}
			}
			private int GetStringPart(String s, int Offset)
			{
				int st = 0;
				for (int i = 0; i < 4; i++)
				{
					st |= s[Offset + i] << (i * 8);
				}
				return st;
			}
			public void Sort()
			{
				SortedDictionary<String, PatTreeNode> alphabet = new SortedDictionary<string, PatTreeNode>();
				foreach (PatTreeNode p in Nodes)
				{
					if (p.name.TrimEnd('\0') == "") continue;
					alphabet.Add(p.name.TrimEnd('\0'), p);
				}
				List<PatTreeNode> AlphabetSortedNodes = new List<PatTreeNode>();
				foreach (PatTreeNode p in alphabet.Values)
				{
					AlphabetSortedNodes.Add(p);
				}
				List<PatTreeNode> LengthSorted = new List<PatTreeNode>();
				for (int j = 0; j < Nodes.Count - 1; j++)
				{
					int longest = -1;
					int longestlength = -1;
					for (int i = 0; i < AlphabetSortedNodes.Count; i++)
					{
						if (AlphabetSortedNodes[i].name.TrimEnd('\0').Length > longestlength)
						{
							longest = i;
							longestlength = AlphabetSortedNodes[i].name.TrimEnd('\0').Length;
						}
					}
					LengthSorted.Add(AlphabetSortedNodes[longest]);
					AlphabetSortedNodes.RemoveAt(longest);
				}
				LengthSorted.Insert(0, Nodes[0]);
				Nodes = LengthSorted;
			}
			public PtreeNode[] GetRawNodes()
			{
				List<PtreeNode> RawNodes = new List<PtreeNode>();
				foreach (PatTreeNode n in Nodes)
				{
					PtreeNode node = new PtreeNode();
					node.refBit = (byte)n.refbit;
					node.idxLeft = (byte)Nodes.IndexOf(n.left);
					node.idxRight = (byte)Nodes.IndexOf(n.right);
					node.idxEntry = (byte)n.idxEntry;
					RawNodes.Add(node);
				}
				return RawNodes.ToArray();
			}
			public static PtreeNode[] Generate(string[] Names)
			{
				PatriciaTreeGenerator p = new PatriciaTreeGenerator();
				for (int i = 0; i < Names.Length; i++)
				{
					p.AddPatTreeNode(Names[i], i);
				}
				p.Sort();
				return p.GetRawNodes();
			}
			public class PatTreeNode
			{
				public Int32 refbit;
				public PatTreeNode left;
				public PatTreeNode right;
				public Int32 idxEntry;
				public String name;
				public override string ToString()
				{
					return name;
				}
			}
		}

		public KeyValuePair<String, T> this[int i]
		{
			get { return new KeyValuePair<string, T>(names[i], entry.data[i]); }
			set { names[i] = value.Key; entry.data[i] = value.Value; }
		}
		public T this[String i]
		{
			get { return entry.data[IndexOf(i)]; }
			set { entry.data[IndexOf(i)] = value; }
		}
		public byte revision = 0;
		public byte numEntry;
		public ushort sizeDictBlk;
		public ushort ofsEntry;
		public class PtreeNode
		{
			public byte refBit;
			public byte idxLeft;
			public byte idxRight;
			public byte idxEntry;
			public void Write(EndianBinaryWriter er)
			{
				er.Write(refBit);
				er.Write(idxLeft);
				er.Write(idxRight);
				er.Write(idxEntry);
			}
		}
		public PtreeNode[] node;
		public class DictEntry
		{
			public DictEntry()
			{
				data = new List<T>();
			}
			public ushort sizeUnit;
			public ushort ofsName;
			public List<T> data;
			public void Write(EndianBinaryWriter er)
			{
				int size = new T().GetDataSize();
				er.Write((UInt16)(size));
				er.Write((UInt16)(4 + size * data.Count));
				foreach (T d in data) d.Write(er);
			}
		}
		public DictEntry entry;

		public List<DictName> names;
	}
	public class DictionaryData
	{
		public virtual UInt16 GetDataSize()
		{
			return 0;
		}
		public virtual void Read(EndianBinaryReader er)
		{

		}

		public virtual void Write(EndianBinaryWriter er)
		{

		}
	}
}
