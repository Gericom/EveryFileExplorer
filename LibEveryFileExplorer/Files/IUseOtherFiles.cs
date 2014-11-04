using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Files
{
	public interface IUseOtherFiles
	{
		void FileOpened(ViewableFile File);
		void FileClosed(ViewableFile File);
	}
}
