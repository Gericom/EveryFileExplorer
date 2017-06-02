using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;
using System.Windows.Forms;

namespace _3DS.NintendoWare.SND
{
    public class CGRP : FileFormat<CGRP.CGRPIdentifier>, IViewable
    {
        public CGRP(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CGRPHeader(er);
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

        public CGRPHeader Header;
        public class CGRPHeader
        {
            public CGRPHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CGRP") throw new SignatureNotCorrectException(Signature, "CGRP", er.BaseStream.Position - 4);
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

        public class CGRPIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Sound;
            }

			public override string GetFileDescription()
			{
				return "CTR Group (CGRP)";
			}

			public override string GetFileFilter()
			{
				return "CTR Group (*.bcgrp)|*.bcgrp";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 45 && File.Data[0] == 'C' && File.Data[1] == 'G' && File.Data[2] == 'R' && File.Data[3] == 'P' && File.Data[0x40] == 'I' && File.Data[0x41] == 'N' && File.Data[0x42] == 'F' && File.Data[0x43] == 'O') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
