using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using System.IO;

namespace _3DS
{
    public class CBMD : FileFormat<CBMD.CBMDIdentifier>, IViewable
    {
        public CBMD(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CBMDHeader(er);
                foreach (var v in Header.Sections)
                {
                    er.BaseStream.Position = v.Offset;
                    switch (v.Id)
                    {

                    }
                }
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

        public CBMDHeader Header;
        public class CBMDHeader
        {
            public CBMDHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CBMD") throw new SignatureNotCorrectException(Signature, "CBMD", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
                Unknown = er.ReadUInt32();
                NrSections = er.ReadUInt32();
                Sections = new SectionInfo[NrSections];
                for (int i = 0; i < NrSections; i++) Sections[i] = new SectionInfo(er);
            }

            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
            public UInt32 Unknown;
            public UInt32 NrSections;
            public SectionInfo[] Sections;
            public class SectionInfo
            {
                public SectionInfo(uint Id) { this.Id = Id; }
                public SectionInfo(EndianBinaryReaderEx er)
                {
                    Id = er.ReadUInt32();
                    Offset = er.ReadUInt32();
                    Size = er.ReadUInt32();
                }
                public void Write(EndianBinaryWriter er)
                {
                    er.Write(Id);
                    er.Write(Offset);
                    er.Write(Size);
                }
                public UInt32 Id;
                public UInt32 Offset;
                public UInt32 Size;
            }
        }

        public class CBMDIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "3DS";
            }

            public override string GetFileDescription()
            {
                return "CTR Banner Model Data (CBMD)";
            }

            public override string GetFileFilter()
            {
                return "CTR Banner Model Data (*.cbmd, *.bnr)|*.cbmd;*.bnr";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'B' && File.Data[2] == 'M' && File.Data[3] == 'D') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
