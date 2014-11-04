using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.UI
{
	public class MainMenu : System.Windows.Forms.MainMenu {
		private System.ComponentModel.IContainer iContainer;
		public MainMenu(System.ComponentModel.IContainer iContainer)
		{
			// TODO: Complete member initialization
			this.iContainer = iContainer;
		}
	}
}
