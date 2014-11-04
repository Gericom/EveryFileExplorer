using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using LibEveryFileExplorer.Compression;

namespace LibEveryFileExplorer.Files
{
	public abstract class EFEFile
	{
		public EFEFile()
		{
			Children = new List<EFEFile>();
		}

		public CompressionFormatBase CompressionFormat { get; private set; }

		public byte[] Data { get; set; }

		public bool IsCompressed { get; private set; }

		public String Name { get; set; }

		public EFEFile Parent { get; set; }

		public List<EFEFile> Children { get; private set; }

		public abstract void Save();

		protected void PrepareDataForUse(byte[] FileData)
		{
			List<Type> Compressions = new List<Type>();
			foreach (dynamic p in EveryFileExplorerUtil.Program.PluginManager.Plugins)
			{
				foreach (Type t in p.CompressionTypes)
				{
					dynamic c = new LibEveryFileExplorer.StaticDynamic(t);
					if (c.Identifier.IsFormat(FileData)) Compressions.Add(t);
				}
			}
			IsCompressed = false;
			CompressionFormat = null;
			if (Compressions.Count == 1)
			{
				CompressionFormatBase c = (CompressionFormatBase)Compressions[0].InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, null, new object[0]);
				try
				{
					byte[] Result = c.Decompress(FileData);
					IsCompressed = true;
					CompressionFormat = c;
					FileData = Result;
				}
				catch { }
			}
			else if (Compressions.Count > 1)
			{
				foreach (Type t in Compressions)
				{
					CompressionFormatBase c = (CompressionFormatBase)t.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, null, new object[0]);
					try
					{
						byte[] Result = c.Decompress(FileData);
						IsCompressed = true;
						CompressionFormat = c;
						FileData = Result;
						break;
					}
					catch { }
				}
			}
			Data = FileData;
		}

		protected byte[] GetDataForSave()
		{
			if (IsCompressed)
			{
				if (CompressionFormat is ICompressable)
				{
					return ((ICompressable)CompressionFormat).Compress(Data);
				}
				else throw new Exception("This file uses a compression method that has no compression implemented!");
			}
			return Data;
		}

		public abstract byte[] FindFileRelative(String Path);

		public abstract override bool Equals(object obj);
	}
}
