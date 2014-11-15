using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _3DS.UI
{
    public partial class ThemeViewer : Form
    {
        ThemeFile theme;
        public ThemeViewer(ThemeFile themeFile)
        {
            theme = themeFile;
            InitializeComponent();
        }

        private void ThemeViewer_Load(object sender, EventArgs e)
        {
            topBackgroundImage.Image = theme.GetTopTexture();
            bottomBackgroundImage.Image = theme.GetBottomTexture();
            folderOpenImage.Image = theme.GetOpenFolderTexture();
            folderClosedImage.Image = theme.GetClosedFolderTexture();
            iconBorder48pxImage.Image = theme.GetIconBorder48px();
            iconBorder24pxImage.Image = theme.GetIconBorder24px();
        }

    }
}
