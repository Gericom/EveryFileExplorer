using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using LibEveryFileExplorer.IO;
using NDS.UI;

namespace NDS
{
	public class BMG:FileFormat<BMG.BMGIdentifier>, IViewable
    {
        public BMG(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new BMGHeader(er);
                //INF1 = INF1Section(er);
                //DAT1 = DAT1Section(er);
            }
            finally
            {
                er.Close();
            }
        }

        public System.Windows.Forms.Form GetDialog()
        {
            return new BMGViewer(this);
        }

        public BMGHeader Header;
        public class BMGHeader
        {
            public BMGHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 8);
                if (Signature != "MESGbmg1") throw new SignatureNotCorrectException(Signature, "MESGbmg1", er.BaseStream.Position - 8);
                Version = er.ReadUInt16();
                NodeNameTableNodeOffset = er.ReadUInt32();
                StringValueTableNodeOffset = er.ReadUInt32();
                RootNodeOffset = er.ReadUInt32();
            }
            public String Signature;
            public UInt16 Version;
            public UInt32 NodeNameTableNodeOffset;
            public UInt32 StringValueTableNodeOffset;
            public UInt32 RootNodeOffset;
        }

        //public class INF1Section INF1Section;
            //{


        public class BMGIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Strings;
			}

			public override string GetFileDescription()
			{
				return "Binary Message Strings (BMG)";
			}

			public override string GetFileFilter()
			{
				return "Binary Message Strings (*.bmg)|*.bmg";
			}

			public override Bitmap GetIcon()
			{
                return Resource.script_text;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 7 && File.Data[0] == 'M' && File.Data[1] == 'E' && File.Data[2] == 'S' && File.Data[3] == 'G' && File.Data[4] == 'b' && File.Data[5] == 'm' && File.Data[6] == 'g' && File.Data[7] == '1') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
