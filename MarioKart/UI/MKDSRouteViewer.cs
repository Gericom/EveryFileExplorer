using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.UI;
using MarioKart.MKDS.NKM;

namespace MarioKart.UI
{
	public partial class MKDSRouteViewer : UserControl, IGameDataSectionViewer
	{
		PATH Routes;
		POIT Points;
		public MKDSRouteViewer(PATH Routes, POIT Points)
		{
			this.Routes = Routes;
			this.Points = Points;
			InitializeComponent();
		}

		private void listViewNF2_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshPoints();
		}

		public event SelectedEventHandler OnSelected;

		public void RefreshListView()
		{
			int[] sel = null;
			if (listViewNF2.SelectedIndices.Count != 0)
			{
				sel = new int[listViewNF2.SelectedIndices.Count];
				listViewNF2.SelectedIndices.CopyTo(sel, 0);
			}
			listViewNF2.BeginUpdate();
			listViewNF2.Items.Clear();
			listViewNF2.Items.AddRange(Routes.GetListViewItems());
			listViewNF2.EndUpdate();
			if (sel != null)
			{
				foreach (int i in sel)
				{
					if (i < Routes.Entries.Count) listViewNF2.SelectedIndices.Add(i);
				}
			}
			RefreshPoints();
		}

		private void RefreshPoints()
		{
			if (listViewNF2.SelectedIndices.Count != 0)
			{
				listViewNF1.BeginUpdate();
				listViewNF1.Items.Clear();
				int idx = 0;
				int q = 0;
				foreach (var o in Routes.Entries)
				{
					if (Points.NrEntries < o.NrPoit + idx) break;
					if (q == listViewNF2.SelectedIndices[0])
					{
						for (int i = 0; i < o.NrPoit; i++)
						{
							var v = Points[idx + i].GetListViewItem();
							v.Text = (idx + i).ToString();
							listViewNF1.Items.Add(v);
						}
						break;
					}
					idx += o.NrPoit;
					q++;
				}
				listViewNF1.EndUpdate();
			}
			else
			{
				listViewNF1.BeginUpdate();
				listViewNF1.Items.Clear();
				listViewNF1.EndUpdate();
			}
		}

		private void MKDSRouteViewer_Load(object sender, EventArgs e)
		{
			listViewNF1.BeginUpdate();
			listViewNF1.Columns.Clear();
			foreach (String s in Points.GetColumnNames()) listViewNF1.Columns.Add(s);
			listViewNF1.EndUpdate();

			listViewNF2.BeginUpdate();
			listViewNF2.Columns.Clear();
			foreach (String s in Routes.GetColumnNames()) listViewNF2.Columns.Add(s);
			listViewNF2.EndUpdate();
			RefreshListView();
		}

		public void UpdateListViewEntry(params object[] Entries)
		{
			//throw new NotImplementedException();
		}

		public void Select(params object[] Entries)
		{
			//throw new NotImplementedException();
		}

		public void RemoveSelection()
		{
			//throw new NotImplementedException();
		}
	}
}
