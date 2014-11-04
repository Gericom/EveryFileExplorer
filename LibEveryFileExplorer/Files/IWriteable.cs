using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Files
{
	public interface IWriteable
	{
		String GetSaveDefaultFileFilter();
		byte[] Write();
	}
}
