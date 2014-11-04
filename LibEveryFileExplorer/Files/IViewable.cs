using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LibEveryFileExplorer.Files
{
	public interface IViewable
	{
		Form GetDialog();
	}
}
