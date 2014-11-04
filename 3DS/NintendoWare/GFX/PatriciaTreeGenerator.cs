using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _3DS.NintendoWare.GFX
{
	internal class PatriciaTreeGenerator
	{
		List<PatTreeNode> Nodes = new List<PatTreeNode>();

		public PatTreeNode[] TreeNodes { get { return Nodes.ToArray(); } }

		private int StringLength;

		public PatriciaTreeGenerator(int StringLength)
		{
			this.StringLength = StringLength;
			AddRootPatTreeNode();
		}
		private PatTreeNode AddRootPatTreeNode()
		{
			PatTreeNode p = new PatTreeNode();
			p.refbit = (uint)(StringLength * 8) - 1;
			p.left = p;
			p.right = p;
			p.idxEntry = 0;
			p.name = new String('\0', StringLength);
			Nodes.Add(p);
			return p;
		}
		public PatTreeNode AddPatTreeNode(String Name, int Index)
		{
			Name = Name.PadRight(StringLength, '\0');
			PatTreeNode n = new PatTreeNode();
			n.name = Name;
			PatTreeNode CurNode = Nodes[0];
			PatTreeNode leftNode = CurNode.left;
			uint bit = (uint)(StringLength * 8) - 1;
			while (CurNode.refbit > leftNode.refbit)
			{
				CurNode = leftNode;
				leftNode = GetBit(Name, leftNode.refbit) ? leftNode.right : leftNode.left;
			}
			while (GetBit(leftNode.name, bit) == GetBit(Name, bit)) bit--;
			CurNode = Nodes[0];
			leftNode = CurNode.left;
			while ((CurNode.refbit > leftNode.refbit) && (leftNode.refbit > bit))
			{
				CurNode = leftNode;
				leftNode = GetBit(Name, leftNode.refbit) ? leftNode.right : leftNode.left;
			}
			n.refbit = bit;
			n.left = GetBit(Name, n.refbit) ? leftNode : n;
			n.right = GetBit(Name, n.refbit) ? n : leftNode;
			if (GetBit(Name, CurNode.refbit)) CurNode.right = n;
			else CurNode.left = n;
			n.idxEntry = Index;
			Nodes.Add(n);
			return n;
		}
		private static bool GetBit(string name, uint bit)
		{
			if (name == null || bit / 8 >= name.Length) throw new ArgumentException();
			return (((int)name[(int)bit / 8] >> ((int)bit & 7)) & 1) == 1;
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

		public static PatriciaTreeGenerator Generate(string[] Names)
		{
			int length = 0;
			foreach (String n in Names) if (n.Length > length) length = n.Length;
			PatriciaTreeGenerator p = new PatriciaTreeGenerator(length);
			for (int i = 0; i < Names.Length; i++)
			{
				p.AddPatTreeNode(Names[i], i);
			}
			p.Sort();
			return p;
		}
		public class PatTreeNode
		{
			public UInt32 refbit;
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
}
