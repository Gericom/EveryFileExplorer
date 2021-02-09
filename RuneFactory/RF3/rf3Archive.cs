using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;

namespace RuneFactory.RF3
{
    public class rf3Archive : FileFormat<rf3Archive.rf3ArchiveIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }
        public class rf3ArchiveIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "RuneFactory 3";
            }

            public override string GetFileDescription()
            {
                return "Rune Factory 3 Archive (rf3Archive.arc)";
            }

            public override string GetFileFilter()
            {
                return "Rune Factory 3 Archive (rf3Archive.arc)|rf3Archive.arc";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Name.Equals("rf3Archive.arc"))
                    return FormatMatch.Content;
                else
                    return FormatMatch.No;
            }

        }

    }
}
