using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using _3DS.UI;
using _3DS.GPU;
using LibEveryFileExplorer.IO;

namespace _3DS.UI
{
    public partial class ThemeViewer : Form
    {
        ThemeFile theme;
        public ThemeViewer(ThemeFile themeFile)
        {
            theme = themeFile;
            InitializeComponent();

//            tabControl1.TabPages[3].Enabled = false;
//            tabControl1.TabPages[3].Visible = false;
            tabControl1.TabPages.Remove(tabPage4);
        }

        private void ThemeViewer_Load(object sender, EventArgs e)
        {
			LoadImages();
            backgroundMusicCheckbox.Checked = theme.header.UseBGMusic;
        }

        private void LoadImages()
        {
            topBackgroundImage.Image = theme.GetTopTexture(clampTextureSizeCheckBox.Checked);
            bottomBackgroundImage.Image = theme.GetBottomTexture(clampTextureSizeCheckBox.Checked);
            folderOpenImage.Image = theme.GetOpenFolderTexture(clampTextureSizeCheckBox.Checked);
            folderClosedImage.Image = theme.GetClosedFolderTexture(clampTextureSizeCheckBox.Checked);
            iconBorder48pxImage.Image = theme.GetIconBorder48px(clampTextureSizeCheckBox.Checked);
            iconBorder24pxImage.Image = theme.GetIconBorder24px(clampTextureSizeCheckBox.Checked);

            if (theme.header.topScreenDrawType == 1)//Solid
                topBackgroundImage.BackColor = theme.topSolidColor;

            if (theme.header.bottomScreenDrawType == 1)//Solid
                bottomBackgroundImage.BackColor = theme.bottomSolidColor;
        }

        private void BitmapFromPngSelect(ref byte[] texturedata, Textures.ImageFormat imageFormat, int fullWidth, int fullHeight, int displayWidth, int displayHeight)
        {
            MessageBox.Show("Please choose a " + fullWidth + "x" + fullHeight + " size image, only " + displayWidth + "x" + displayHeight + " will be displayed on the 3ds!");
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PNG Files (*.png)|*.png";

            DialogResult result = dialog.ShowDialog();

            Console.WriteLine(result.ToString() + " " + dialog.FileName.Length);
            if (result == DialogResult.OK && dialog.FileName.Length > 0)
            {
                Bitmap bitmap = new Bitmap(new MemoryStream(File.ReadAllBytes(dialog.FileName)));
                texturedata = Textures.FromBitmap(bitmap, imageFormat, true);
				LoadImages();
            }
        }

        private void folderOpenImage_Click(object sender, EventArgs e)
        {
			BitmapFromPngSelect(ref theme.openFolderTexture, Textures.ImageFormat.RGB8, theme.folderWidth, theme.folderHeight, 82, 64);			
        }
        private void folderClosedImage_Click(object sender, EventArgs e)
        {
			BitmapFromPngSelect(ref theme.closedFolderTexture, Textures.ImageFormat.RGB8, theme.folderWidth, theme.folderHeight, 74, 64);		
        }
        private void topBackgroundImage_Click(object sender, EventArgs e)
        {
			BitmapFromPngSelect(ref theme.topScreenTexture, Textures.ImageFormat.RGB565, theme.topWidth, theme.topHeight, theme.topClampWidth, theme.topClampHeight);
        }

        private void bottomBackgroundImage_Click(object sender, EventArgs e)
        {
			BitmapFromPngSelect(ref theme.bottomScreenTexture, Textures.ImageFormat.RGB565, theme.bottomWidth, theme.bottomHeight, theme.bottomClampWidth, theme.bottomClampHeight);
        }

        private void iconBorder48pxImage_Click(object sender, EventArgs e)
        {
			BitmapFromPngSelect(ref theme.iconBorder48pxTexture, Textures.ImageFormat.RGB8, theme.iconBorder48pxWidth, theme.iconBorder48pxHeight, 36, 72);
        }

        private void iconBorder24pxImage_Click(object sender, EventArgs e)
        {
			BitmapFromPngSelect(ref theme.iconBorder24pxTexture, Textures.ImageFormat.RGB8, theme.iconBorder24pxWidth, theme.iconBorder24pxHeight, 25, 50);
        }
    
        private void clampTextureSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
			LoadImages();
        }

        private void backgroundMusicCheckbox_CheckedChanged(object sender, EventArgs e)
        {
			theme.header.UseBGMusic = backgroundMusicCheckbox.Checked;
        }

    }
}
