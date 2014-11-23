using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _3DS.UI
{
	public partial class SAHTViewer : Form
	{
		public SARCHashTable HashTable;
		public SAHTViewer(SARCHashTable HashTable)
		{
			this.HashTable = HashTable;
			InitializeComponent();
		}

		private void SAHTViewer_Load(object sender, EventArgs e)
		{
			UpdateEntries();
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listView1.SelectedIndices.Count != 0) toolStripButton2.Enabled = true;
			else toolStripButton2.Enabled = false;
		}

		private void UpdateEntries()
		{
			listView1.BeginUpdate();
			listView1.Items.Clear();
			foreach (var v in HashTable.Entries)
			{
				listView1.Items.Add(new ListViewItem(new string[] { v.Name, "0x" + v.Hash.ToString("X8") }));
			}
			listView1.EndUpdate();
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			String name = Microsoft.VisualBasic.Interaction.InputBox("Please give the name or path of the file:", "New Entry");
			if (name == null || name.Length == 0) return;
			if (HashTable.GetEntryByName(name) != null)
			{
				MessageBox.Show("This name is already in the table!");
				return;
			}
			HashTable.Entries.Add(new SARCHashTable.SAHTEntry(name));
			UpdateEntries();
		}

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			HashTable.Entries.RemoveAt(listView1.SelectedIndices[0]);
			UpdateEntries();
		}

		private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			List<SARCHashTable.SAHTEntry> list = new List<SARCHashTable.SAHTEntry>();
			foreach (var v in HashTable.Entries)
			{
				if (HashTable.GetEntryByHash(v.Hash) != v) continue;
				list.Add(v);
			}
			HashTable.Entries = list;

			HashTable.Entries.Sort((v1, v2) => v1.Name.CompareTo(v2.Name));
			UpdateEntries();
		}
	}
}
