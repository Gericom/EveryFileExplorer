using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using _3DS.UI;

namespace _3DS
{
    public class ThemeIconFile : FileFormat<ThemeIconFile.ThemeIconIdentifier>, IViewable
    {
        public ThemeIconFile(byte[] Data)
        {
            icon = Data;
            width = 48;
            height = 48;
        }

        int width;
        int height;
        byte[] icon;

        public Bitmap getIconTexture()
        {
            return GPU.Textures.ToBitmap(icon, width, height, GPU.Textures.ImageFormat.RGB565, true);
        }

        public System.Windows.Forms.Form GetDialog()
        {
            return new ThemeIconViewer(this);
        }


        public class ThemeIconIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "3DS Themes";
            }

            public override string GetFileDescription()
            {
                return "3DS System Menu Theme Icon";
            }

            public override string GetFileFilter()
            {
                return "3DS System Menu Icon (*.icn)|*.icn";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Name.EndsWith(".icn"))
                    return FormatMatch.Extension;
                else
                    return FormatMatch.No;
            }

        }

    }

}
