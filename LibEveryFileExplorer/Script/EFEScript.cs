using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.CodeDom;

namespace LibEveryFileExplorer.Script
{
	public class EFEScript
	{
		private static Dictionary<String, Delegate> Commands = new Dictionary<String, Delegate>();

		public static void Execute(String Script, params string[] Args)
		{
			var comp = new CSharpCodeProvider();
			//prepare the script to be executed!
			StringBuilder b = new StringBuilder();
			b.AppendLine("using System;");
			b.AppendLine("using System.IO;");
			b.AppendLine();
			foreach (var v in Commands)
			{
				String Name;
				string[] parts = v.Key.Split('.');
				if (parts.Length > 2)
				{
					b.Append("namespace ");
					for (int i = 0; i < parts.Length - 2; i++)
					{
						if (i != 0) b.Append(".");
						b.Append(parts[i]);
					}
					b.AppendLine();
					b.AppendLine("{");
					{
						b.AppendLine("public partial class " + parts[parts.Length - 2]);
						b.AppendLine("{");
					}
					Name = parts[parts.Length - 1];
				}
				else
				{
					b.AppendLine("namespace EFEScript");
					b.AppendLine("{");
					if (parts.Length > 1)
					{
						b.AppendLine("public partial class " + parts[0]);
						b.AppendLine("{");
						Name = parts[1];
					}
					else
					{
						b.AppendLine("public partial class Script");
						b.AppendLine("{");
						Name = parts[0];
					}
				}
				MethodInfo m = v.Value.Method;
				if (m.ReturnType.FullName == "System.Void") b.Append("public static void ");
				else b.Append("public static " + m.ReturnType.FullName + " ");
				b.Append(Name + "(");
				bool first = true;
				foreach (var vv in m.GetParameters())
				{
					if (!first) b.Append(", ");
					b.Append(vv.ParameterType.FullName + " ");
					b.Append(vv.Name);
					first = false;
				}
				b.AppendLine(")");
				b.AppendLine("{");
				{
					if (m.ReturnType.FullName != "System.Void") b.Append("return ");
					b.Append(m.DeclaringType.FullName + "." + m.Name + "(");
					first = true;
					foreach (var vv in m.GetParameters())
					{
						if (!first) b.Append(", ");
						b.Append(vv.Name);
						first = false;
					}
					b.AppendLine(");");
				}
				b.AppendLine("}");
				b.AppendLine("}");
				b.AppendLine("}");
			}
			b.AppendLine();
			b.AppendLine("namespace EFEScript");
			b.AppendLine("{");
			{
				b.AppendLine("public partial class Script");
				b.AppendLine("{");
				{
					b.AppendLine("public static void Run(params string[] Args)");
					b.AppendLine("{");
					b.AppendLine(Script);
					b.AppendLine("}");
				}
				b.AppendLine("}");
			}
			b.AppendLine("}");

			var opt = new CompilerParameters();
			opt.GenerateInMemory = true;
			opt.GenerateExecutable = false;
			opt.OutputAssembly = null;
		 	AssemblyName[] refassem = Assembly.GetEntryAssembly().GetReferencedAssemblies();
			foreach (var a in refassem)
			{
				opt.ReferencedAssemblies.Add(a.Name + ".dll");
			}
			foreach (var v in Commands)
			{
				opt.ReferencedAssemblies.Add(v.Value.Method.DeclaringType.Assembly.Location);
			}
			var result = comp.CompileAssemblyFromSource(opt, b.ToString());
			if (result.Errors.Count != 0)
			{
				throw new Exception("An error has occured while trying to compile the script: " + result.Errors[0].ToString());
			}
			result.CompiledAssembly.GetType("EFEScript.Script").InvokeMember("Run", BindingFlags.InvokeMethod, null, null, Args);
		}

		public static void RegisterCommand(String Command, Delegate Function)
		{
			if (Commands.ContainsKey(Command) || Commands.ContainsValue(Function)) throw new Exception("Command '" + Command + "' or Function already registered!");
			Commands.Add(Command, Function);
		}
	}
}
