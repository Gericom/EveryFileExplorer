using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;
using System.IO;
using System.Windows.Forms;

namespace _3DS.NintendoWare.H3D
{
	public class BCH : FileFormat<BCH.BCHIdentifier>, IViewable
	{
		public BCH(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new BCHHeader(er);
				Content = new BCHContent(er);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new Form();
		}

		public BCHHeader Header;
		public class BCHHeader
		{
			public BCHHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
			[BinaryStringSignature("BCH\0")]
			[BinaryFixedSize(4)]
			public String Signature;
			public Byte BackwardCompatibility;
			public Byte ForwardCompatibility;
			public UInt16 Version;
			public UInt32 ContentOffset;
			public UInt32 StringTableOffset;
			public UInt32 CommandDataOffset;
			public UInt32 RawDataOffset;
			public UInt32 RawDataExtendedOffset;
			public UInt32 RelocatableTableOffset;
			public UInt32 ContentSize;
			public UInt32 StringTableSize;
			public UInt32 CommandDataSize;
			public UInt32 RawDataSize;
			public UInt32 RawDataExtendedSize;
			public UInt32 RelocatableTableSize;
			public UInt32 UninitializedDataSectionSize;
			public UInt32 UninitializedCommandSectionSize;
			public UInt16 Flags;
			public UInt16 PhysicalAddressCount;
		}

		public BCHContent Content;
		public class BCHContent
		{
			public BCHContent(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}

			[BinaryFixedSize(12)]
			public ContentTableEntry[] ContentTable;
			public class ContentTableEntry
			{
				public ContentTableEntry(EndianBinaryReaderEx er)
				{
					er.ReadObject(this);
				}
				public UInt32 Offset;
				public UInt32 NrEntries;
				public UInt32 Unknown2;
			}

			public class ModelContent
			{
				public Byte Flags;
				public Byte SkeletonScalingType;
				public UInt16 NrSilhouetteMaterials;
				[BinaryFixedSize(3 * 4)]
				public Single WorldMatrix;
				//materials
			}

			public class MaterialData
			{
				public UInt32 ContentOffset;
				public UInt32 Texture0Offset;
				public UInt32 Texture1Offset;
				public UInt32 Texture2Offset;
				//Texture Commands
			}
		}

		public class BCHIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Models;
			}

			public override string GetFileDescription()
			{
				return "Binary CTR H3D Resource (BCH)";
			}

			public override string GetFileFilter()
			{
				return "Binary CTR H3D Resource (*.bch)|*.bch";
			}

			public override Bitmap GetIcon()
			{
				return Resource.leaf;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'C' && File.Data[2] == 'H' && File.Data[3] == 0) return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
