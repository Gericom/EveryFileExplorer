using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LibEveryFileExplorer.Projects
{
	public abstract class Project
	{
		public Project(String ProjectDir)
		{
			ProjectDirectory = ProjectDir;
		}

		public String ProjectDirectory { get; private set; }
		public abstract bool CanRun { get; }

		public abstract Control GetProjectControl();

		public abstract void Build();
		public abstract void Run();
	}
}
