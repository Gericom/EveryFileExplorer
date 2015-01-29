using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.GameData;

namespace LibEveryFileExplorer.UI
{
	public delegate void SelectedEventHandler(IGameDataSectionViewer Viewer, object Entry);

	public interface IGameDataSectionViewer
	{
		event SelectedEventHandler OnSelected;

		void RefreshListView();
		void UpdateListViewEntry(object Entry);
		void Select(object Entry);
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
		}

		void listViewNF1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewNF1.SelectedIndices.Count != 0 && OnSelected != null) OnSelected.Invoke(this, Section[listViewNF1.SelectedIndices[0]]);
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
			int sel = -1;
			if (listViewNF1.SelectedIndices.Count != 0) sel = listViewNF1.SelectedIndices[0];
			listViewNF1.BeginUpdate();
			listViewNF1.Items.Clear();
			listViewNF1.Items.AddRange(Section.GetListViewItems());
			listViewNF1.EndUpdate();
			if (sel != -1 && sel < Section.Entries.Count) listViewNF1.SelectedIndices.Add(sel);
			else if (sel != -1) listViewNF1.SelectedIndices.Add(Section.Entries.Count - 1);
		}

		public void UpdateListViewEntry(object Entry)
		{
			if (!(Entry is T)) return;
			int idx = Section.Entries.IndexOf((T)Entry);
			if (idx < 0) return;
			int sel = -1;
			if (listViewNF1.SelectedIndices.Count != 0) sel = listViewNF1.SelectedIndices[0];
			listViewNF1.BeginUpdate();
			listViewNF1.Items[idx] = ((T)Entry).GetListViewItem();
			listViewNF1.Items[idx].Text = idx.ToString();
			listViewNF1.EndUpdate();
			if (sel != -1 && sel < Section.Entries.Count) listViewNF1.SelectedIndices.Add(sel);
			else if (sel != -1) listViewNF1.SelectedIndices.Add(Section.Entries.Count - 1);
		}

		public void Select(object Entry)
		{
			RemoveSelection();
			if (!(Entry is T)) return;
			int idx = Section.Entries.IndexOf((T)Entry);
			listViewNF1.SelectedIndices.Add(idx);
			UpdateListViewEntry(Entry);
		}

		public void RemoveSelection()
		{
			listViewNF1.SelectedItems.Clear();
		}
	}
}
