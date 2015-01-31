using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.GameData;
using System.Windows.Forms;

namespace LibEveryFileExplorer.UI
{
	public delegate void SelectedEventHandler(IGameDataSectionViewer Viewer, object[] Entries);

	public interface IGameDataSectionViewer
	{
		event SelectedEventHandler OnSelected;

		void RefreshListView();
		void UpdateListViewEntry(params object[] Entries);
		void Select(params object[] Entries);
		void RemoveSelection();
	}

	public class GameDataSectionViewer<T> : GameDataSectionViewerBase, IGameDataSectionViewer where T : GameDataSectionEntry, new()
	{
		public event SelectedEventHandler OnSelected;

		GameDataSection<T> Section;
		public GameDataSectionViewer(GameDataSection<T> Section)
			: base()
		{
			this.Section = Section;
			base.Load += new EventHandler(GameDataSectionViewer_Load);
			base.listViewNF1.SelectedIndexChanged += new EventHandler(listViewNF1_SelectedIndexChanged);
			base.buttonAdd.Click += new EventHandler(buttonAdd_Click);
			base.buttonRemove.Click += new EventHandler(buttonRemove_Click);
			buttonRemove.Enabled = buttonUp.Enabled = buttonDown.Enabled = false;
			t = new Timer();
			t.Tick += new EventHandler(t_Tick);
		}

		void t_Tick(object sender, EventArgs e)
		{
			t.Enabled = false;
			if (listViewNF1.SelectedIndices.Count != 0 && OnSelected != null)
			{
				List<T> entries = new List<T>();
				foreach (int a in listViewNF1.SelectedIndices) entries.Add(Section.Entries[a]);
				OnSelected(this, entries.ToArray());
			}
			if (listViewNF1.SelectedIndices.Count != 0) buttonRemove.Enabled = buttonUp.Enabled = buttonDown.Enabled = true;
			else buttonRemove.Enabled = buttonUp.Enabled = buttonDown.Enabled = false;
			if (!RemovingSelection && listViewNF1.SelectedIndices.Count == 0 && OnSelected != null) OnSelected(null, null);
		}

		Timer t;

		void listViewNF1_SelectedIndexChanged(object sender, EventArgs e)
		{
			t.Interval = 100;
			t.Enabled = true;
		}

		void buttonRemove_Click(object sender, EventArgs e)
		{
			if (listViewNF1.SelectedIndices.Count != 0)
			{
				List<T> entries = new List<T>();
				foreach (int a in listViewNF1.SelectedIndices) entries.Add(Section.Entries[a]);
				listViewNF1.SelectedIndices.Clear();
				foreach (T a in entries)
				{
					Section.Entries.Remove(a);
					Section.NrEntries--;
				}
				RefreshListView();
			}
		}

		void buttonAdd_Click(object sender, EventArgs e)
		{
			T tmp = new T();
			Section.Entries.Add(tmp);
			Section.NrEntries++;
			listViewNF1.BeginUpdate();
			listViewNF1.Items.Add(tmp.GetListViewItem());
			listViewNF1.EndUpdate();
			Select(tmp);
		}

		

		void GameDataSectionViewer_Load(object sender, EventArgs e)
		{
			listViewNF1.BeginUpdate();
			listViewNF1.Columns.Clear();
			foreach (String s in Section.GetColumnNames()) listViewNF1.Columns.Add(s);
			listViewNF1.EndUpdate();
			RefreshListView();
		}

		public void RefreshListView()
		{
			int[] sel = null;
			if (listViewNF1.SelectedIndices.Count != 0)
			{
				sel = new int[listViewNF1.SelectedIndices.Count];
				listViewNF1.SelectedIndices.CopyTo(sel, 0);
			}
			listViewNF1.BeginUpdate();
			listViewNF1.Items.Clear();
			listViewNF1.Items.AddRange(Section.GetListViewItems());
			listViewNF1.EndUpdate();
			if (sel != null)
			{
				foreach (int i in sel)
				{
					if (i < Section.Entries.Count) listViewNF1.SelectedIndices.Add(i);
				}
			}
		}

		public void UpdateListViewEntry(params object[] Entries)
		{
			foreach (object Entry in Entries)
			{
				if (!(Entry is T)) continue;
				int idx = Section.Entries.IndexOf((T)Entry);
				if (idx < 0) continue;
				int[] sel = null;
				if (listViewNF1.SelectedIndices.Count != 0)
				{
					sel = new int[listViewNF1.SelectedIndices.Count];
					listViewNF1.SelectedIndices.CopyTo(sel, 0);
				}
				RemovingSelection = true;
				listViewNF1.BeginUpdate();
				listViewNF1.Items[idx] = ((T)Entry).GetListViewItem();
				listViewNF1.Items[idx].Text = idx.ToString();
				listViewNF1.EndUpdate();
				RemovingSelection = false;
				if (sel != null)
				{
					foreach (int i in sel)
					{
						if (i < Section.Entries.Count) listViewNF1.SelectedIndices.Add(i);
					}
				}
			}
		}

		public void Select(params object[] Entries)
		{
			RemoveSelection();
			int visidx = -1;
			foreach (object Entry in Entries)
			{
				if (!(Entry is T)) continue;
				int idx = Section.Entries.IndexOf((T)Entry);
				if (visidx < 0) visidx = idx;
				listViewNF1.SelectedIndices.Add(idx);
			}
			UpdateListViewEntry(Entries);
			if (visidx >= 0) listViewNF1.EnsureVisible(visidx);
		}
		bool RemovingSelection = false;
		public void RemoveSelection()
		{
			RemovingSelection = true;
			listViewNF1.SelectedItems.Clear();
			RemovingSelection = false;
		}
	}
}
