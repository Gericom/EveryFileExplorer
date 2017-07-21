using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.SND
{
    public class STRM : FileFormat<STRM.STRMIdentifier>
    {
        public class STRMIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return null;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Stream (STRM)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Stream (*.strm)|*.strm";
            }

            public override Bitmap GetIcon()
            {
                return Resource.speaker;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'T' && File.Data[2] == 'R' && File.Data[3] == 'M') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
