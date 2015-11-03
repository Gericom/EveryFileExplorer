using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CommonFiles.Maya
{
	public class MayaASCIIWriter
	{
		private StringWriter mWriter;

		public MayaASCIIWriter()
		{
			mWriter = new StringWriter();
			WriteComment("Maya ASCII 3.0 scene");
			WriteComment("Created by Every File Explorer");
			mWriter.WriteLine();
			BeginStatement("requires");
			{
				WriteArgument("maya", "\"3.0\"");
			}
			EndStatement();
			BeginStatement("currentUnit");
			{
				WriteArgument("-l", "centimeter");
				WriteArgument("-a", "degree");
				WriteArgument("-t", "film");
			}
			EndStatement();
		}

		public void WriteComment(String comment)
		{
			mWriter.WriteLine("//" + comment);
		}

		public void BeginStatement(String keyword)
		{
			mWriter.Write(keyword);
		}

		public void WriteArgument(int val)
		{
			WriteArgument(val.ToString());
		}

		public void WriteArgument(float val)
		{
			WriteArgument(val.ToString().Replace(",", "."));
		}

		public void WriteArgument(String arg)
		{
			mWriter.Write(" " + arg);
		}

		public void WriteArgument(String arg, int val)
		{
			WriteArgument(arg, val.ToString());
		}

		public void WriteArgument(String arg, float val)
		{
			WriteArgument(arg, val.ToString().Replace(",", "."));
		}

		public void WriteArgument(String arg, String val)
		{
			mWriter.Write(" " + arg + " " + val);
		}

		public void EndStatement()
		{
			mWriter.WriteLine(";");
		}

		public void CreateNode(String Type, String Name)
		{
			CreateNode(Type, Name, null, false, false);
		}

		public void CreateNode(String Type, String Name, bool Shared)
		{
			CreateNode(Type, Name, null, Shared, false);
		}

		public void CreateNode(String Type, String Name, String Parent)
		{
			CreateNode(Type, Name, Parent, false, false);
		}

		public void CreateNode(String Type, String Name, String Parent, bool Shared)
		{
			CreateNode(Type, Name, Parent, Shared, false);
		}

		public void CreateNode(String Type, String Name, String Parent, bool Shared, bool SkipSelect)
		{
			BeginStatement("createNode");
			{
				WriteArgument(Type);
				WriteArgument("-n", "\"" + Name + "\"");
				if (Parent != null) WriteArgument("-p", "\"" + Parent + "\"");
				if (Shared) WriteArgument("-s");
				if (SkipSelect) WriteArgument("-ss");
			}
			EndStatement();
		}

		public void ConnectAttribute(String FirstNode, String SecondNode)
		{
			ConnectAttribute(FirstNode, SecondNode, false);
		}

		public void ConnectAttribute(String FirstNode, String SecondNode, bool NextAvailable)
		{
			BeginStatement("connectAttr");
			{
				WriteArgument("\"" + FirstNode + "\"");
				WriteArgument("\"" + SecondNode + "\"");
				if (NextAvailable) WriteArgument("-na");
			}
			EndStatement();
		}

		public String Close()
		{
			mWriter.Flush();
			String result = mWriter.ToString();
			mWriter.Close();
			return result;
		}
	}
}
