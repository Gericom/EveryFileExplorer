using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LibEveryFileExplorer.Projects
{
	public interface ProjectBase
	{
		String ProjectDirectory { get; }
		bool CanRun { get; }

		TabPage[] GetProjectTabPages();

		void Build();
		void Run();
		void SaveProjectFile();
	}

	public abstract class Project<T> : ProjectBase where T : ProjectIdentifier, new()
	{
		private static T _identifier = new T();
		public static T Identifier { get { return _identifier; } }

		public Project(String ProjectDir)
		{
			ProjectDirectory = ProjectDir;
		}

		public String ProjectDirectory { get; private set; }
		public abstract bool CanRun { get; }

		public abstract TabPage[] GetProjectTabPages();

		public abstract void Build();
		public abstract void Run();
		public abstract void SaveProjectFile();
	}

	public abstract class ProjectIdentifier
	{
		public abstract String GetProjectDescription();
		public abstract bool IsProject(byte[] Data);
	}
}
