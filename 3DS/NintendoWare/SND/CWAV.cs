using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.SND;
using CommonFiles;
using LibEveryFileExplorer.IO;
using _3DS.UI;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace _3DS.NintendoWare.SND
{
    public class CWAV:FileFormat<CWAV.CWAVIdentifier>, IViewable
    {
        public CWAV(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CWAVHeader(er);
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

        public CWAVHeader Header;
        public class CWAVHeader
        {
            public CWAVHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CWAV") throw new SignatureNotCorrectException(Signature, "CWAV", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
                FileSize = er.ReadUInt32();
                FileTableOffset = er.ReadUInt32();
                FileTableLength = er.ReadUInt32();
                FileDataOffset = er.ReadUInt32();
            }
            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
            public UInt32 FileSize;
            public UInt32 FileTableOffset;
            public UInt32 FileTableLength;
            public UInt32 FileDataOffset;
        }

        public struct CWAVChannelDataPointer
        {
            public uint Flags;
            public uint Offset;
        }

        public struct CWAVChannelData
        {
            public uint Flags;
            public ulong Offset;
            public uint FFs;
            public uint Padding;
        }

        public struct CWAVINFO
        {
            public char[] Magic;
            public uint InfoDataLength;
            public uint Type;
            public uint SampleRate;
            public uint Unknown0;
            public uint NumberOfSamples;
            public uint Unknown2;
            public uint Channels;
        }

        public class CWAVIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Audio;
            }

			public override string GetFileDescription()
			{
				return "CTR Wave (CWAV)";
			}

			public override string GetFileFilter()
			{
				return "CTR Wave (*.bcwav)|*.bcwav";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'V') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
