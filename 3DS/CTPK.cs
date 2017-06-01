using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;

namespace _3DS
{
    public class CTPK:FileFormat<CTPK.CTPKIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class CTPKIdentifier : FileFormatIdentifier
        {
             public override string GetCategory()
            {
                return "CTPK Archives";
            }

                public override string GetFileDescription()
            {
                return "CTR Texture Package (CTPK)";
            }

            public override string GetFileFilter()
            {
                return "CTR Texture Package (*.ctpk)|*.ctpk";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'T' && File.Data[2] == 'P' && File.Data[3] == 'K') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
