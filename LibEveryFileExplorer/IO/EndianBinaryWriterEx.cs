using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibEveryFileExplorer.IO
{
	public class EndianBinaryWriterEx : EndianBinaryWriter
	{
		public EndianBinaryWriterEx(Stream baseStream)
			: base(baseStream) { }

		public EndianBinaryWriterEx(Stream baseStream, Endianness endianness)
			: base(baseStream, endianness) { }

		private struct SizeSection
		{
			public long StartAddress;
			public int SizeOffset;
		}

		private Stack<SizeSection> SizeSections = new Stack<SizeSection>();

		public void BeginSizeSection(int SizeOffset = 0)
		{
			SizeSections.Push(new SizeSection() { StartAddress = BaseStream.Position, SizeOffset = SizeOffset });
		}

		public void EndSizeSection()
		{
			SizeSection s = SizeSections.Pop();
			uint length = (uint)(BaseStream.Position - s.StartAddress);
			long curpos = BaseStream.Position;
			BaseStream.Position = s.StartAddress + s.SizeOffset;
			Write(length);
			BaseStream.Position = curpos;
		}

		public void WritePadding(int Alignment, byte PadChar = 0)
		{
			while ((BaseStream.Position % Alignment) != 0) Write(PadChar);
		}
	}
}
