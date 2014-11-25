using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LibEveryFileExplorer.GameData
{
	public abstract class GameDataSection<T> where T : GameDataSectionEntry
	{
		public String Signature;
		public UInt32 NrEntries;
		public List<T> Entries = new List<T>();

		public abstract String[] GetColumnNames();
		public ListViewItem[] GetListViewItems()
		{
			ListViewItem[] items = new ListViewItem[Entries.Count];
			for (int i = 0; i < NrEntries; i++)
			{
				items[i] = Entries[i].GetListViewItem();
				items[i].Text = i.ToString();
			}
			return items;
		}
		public T this[int index]
		{
			get { return Entries[index]; }
			set { Entries[index] = value; }
		}
	}

	public abstract class GameDataSectionEntry
	{
		public virtual void Write(EndianBinaryReader er)
		{
			throw new NotImplementedException();
		}

		public abstract ListViewItem GetListViewItem();

		protected static String GetHexReverse(sbyte Value)
		{
			return (Value & 0xFF).ToString("X2");
		}
		protected static String GetHexReverse(byte Value)
		{
			return Value.ToString("X2");
		}
		protected static String GetHexReverse(short Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
		protected static String GetHexReverse(ushort Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
		protected static String GetHexReverse(int Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
		protected static String GetHexReverse(uint Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
	}
}
