using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using LibEveryFileExplorer.Files;
using System.Xml.Schema;
using System.Xml;
using System.IO;

namespace LibEveryFileExplorer.Projects
{
	public interface ProjectBase
	{
		bool CreateNew();
		void Save();

		void Build();

		TreeNode[] GetProjectTree();
		TabPage[] GetProjectTabPages();

		String ProjectDir { get; }
	}

	public abstract class Project<T, U> : ProjectBase
		where T : ProjectIdentifier, new()
		where U : ProjectFile, new()
	{
		private static T _identifier = new T();
		public static T Identifier { get { return _identifier; } }

		public Project() { }

		public Project(String ProjectFile)
		{
			ProjectDir = Path.GetDirectoryName(ProjectFile);
			this.ProjectFile = Projects.ProjectFile.FromByteArray<U>(File.ReadAllBytes(ProjectFile));
		}

		public abstract bool CreateNew();
		public void Save()
		{
			ProjectFile.ProjectClass = this.GetType().AssemblyQualifiedName;
			byte[] data = ProjectFile.Write();
			File.Create(ProjectDir + "\\" + ProjectFile.ProjectName + ".efeproj").Close();
			File.WriteAllBytes(ProjectDir + "\\" + ProjectFile.ProjectName + ".efeproj", data);
		}

		public abstract void Build();

		public abstract TreeNode[] GetProjectTree();
		public virtual TabPage[] GetProjectTabPages()
		{
			return new TabPage[0];
		}

		public U ProjectFile { get; protected set; }
		public String ProjectDir { get; protected set; }
	}

	public abstract class ProjectIdentifier
	{
		public abstract String GetProjectDescription();
	}

	[Serializable]
	[XmlRoot("EFEProject")]
	public abstract class ProjectFile
	{
		public ProjectFile() { }
		public ProjectFile(String ProjectName) { this.ProjectName = ProjectName; }

		[XmlAttribute("Name")]
		public String ProjectName { get; set; }

		[XmlAttribute("Class")]
		public String ProjectClass { get; set; }

		public static T FromByteArray<T>(byte[] Data)
		{
			XmlSerializer s = new XmlSerializer(typeof(T));
			return (T)s.Deserialize(new MemoryStream(Data));
		}

		public static Type GetProjectType(byte[] Data)
		{
			XmlReader r = XmlReader.Create(new MemoryStream(Data));
			r.Read();
			r.Read();
			r.Read();
			r.MoveToAttribute("Class");
			String s = r.ReadContentAsString();
			r.Close();
			return Type.GetType(s);
		}

		public byte[] Write()
		{
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("", "");
			XmlSerializer s = XmlSerializer.FromTypes(new[] { this.GetType() })[0];
			MemoryStream m = new MemoryStream();
			s.Serialize(m, this, ns);
			byte[] data = m.ToArray();
			m.Close();
			return data;
		}
	}
}
