using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LibEveryFileExplorer.IO;

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
		public virtual void Write(EndianBinaryWriter er)
		{
			throw new NotImplementedException();
		}

		public abstract ListViewItem GetListViewItem();
	}
}
