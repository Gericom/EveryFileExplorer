using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.GameData;

namespace LibEveryFileExplorer.UI
{
	public class GameDataSectionViewer<T> : GameDataSectionViewerBase where T : GameDataSectionEntry, new()
	{
		GameDataSection<T> Section;
		public GameDataSectionViewer(GameDataSection<T> Section)
			: base()
		{
			this.Section = Section;
			base.Load += new EventHandler(GameDataSectionViewer_Load);
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
			listViewNF1.BeginUpdate();
			listViewNF1.Items.Clear();
			listViewNF1.Items.AddRange(Section.GetListViewItems());
			listViewNF1.EndUpdate();
		}
	}
}
