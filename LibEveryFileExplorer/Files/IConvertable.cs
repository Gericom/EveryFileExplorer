using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Files
{
	public interface IConvertable
	{
		String GetConversionFileFilters();
		bool Convert(int FilterIndex, String Path);
	}
}
