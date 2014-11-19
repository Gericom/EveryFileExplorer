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
using System.Timers;

namespace _3DS.UI
{
    public partial class ThemeViewer : Form
    {
        ThemeFile theme;
        public ThemeViewer(ThemeFile themeFile)
        {
            theme = themeFile;
            InitializeComponent();
            tabControl1.TabPages.Remove(tabPage4);
        }

        private void ThemeViewer_Load(object sender, EventArgs e)
        {
            loadImages();
            backgroundMusicCheckbox.Checked = theme.header.backgroundMusicEnabled;
        }

        private void loadImages()
        {
			if (theme.header.topScreenDrawType == 2)
			{
				topBackgroundImage.BackColor = Color.Aqua;
				topBackgroundImage.Image = theme.GetTopAlphaTexture1();
			}
			else
			{ 
			    topBackgroundImage.Image = theme.GetTopTexture(clampTextureSizeCheckBox.Checked);
				simTopBackgroundImage.Image = topBackgroundImage.Image;
			}

			bottomBackgroundImage.Image = theme.GetBottomTexture(clampTextureSizeCheckBox.Checked);
			simBottomBackgroundImage.Image = bottomBackgroundImage.Image;

			if (theme.header.useFolderTextures)
			{
				folderOpenImage.Image = theme.GetOpenFolderTexture(clampTextureSizeCheckBox.Checked);
				folderClosedImage.Image = theme.GetClosedFolderTexture(clampTextureSizeCheckBox.Checked);
			}
			if (theme.header.useIconBorders)
			{
	            iconBorder48pxImage.Image = theme.GetIconBorder48px(clampTextureSizeCheckBox.Checked);
				iconBorder24pxImage.Image = theme.GetIconBorder24px(clampTextureSizeCheckBox.Checked);
			}
    
			if (theme.header.topScreenDrawType == 1)//Solid
                topBackgroundImage.BackColor = theme.topSolidColor;

            if (theme.header.bottomScreenDrawType == 1)//Solid
                bottomBackgroundImage.BackColor = theme.bottomSolidColor;

			if (theme.header.bottomScreenFrameType == 2 || theme.header.bottomScreenFrameType == 4)
			{
				Rectangle frame0rect = new Rectangle(0, 0, 320, theme.bottomClampHeight);
				Rectangle frame1rect = new Rectangle(320, 0, 320, theme.bottomClampHeight);
				Rectangle frame2rect = new Rectangle(640, 0, 320, theme.bottomClampHeight);

				Bitmap map = new Bitmap(960, 240);//new Bitmap(bottomBackgroundImage.Image);
				bottomBackgroundImage.DrawToBitmap(map, new Rectangle(0,0,960,240));
					bottomFrame0 = map.Clone(frame0rect, map.PixelFormat);
					bottomFrame1 = map.Clone(frame1rect, map.PixelFormat);
					bottomFrame2 = map.Clone(frame2rect, map.PixelFormat);

			}

			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Elapsed += new System.Timers.ElapsedEventHandler(onTimedEvent);
			timer.Interval = 500;
			timer.Enabled = true;
        }


		Bitmap bottomFrame0;
		Bitmap bottomFrame1;
		Bitmap bottomFrame2;
		int frameptr = 0;
		int testptr = 0;
		private void onTimedEvent(object source, ElapsedEventArgs e)
		{//normally only animates on right/left movement

			//TESTING
			if (theme.header.topScreenDrawType == 2)
			{
				if (testptr == 2)
					testptr = 0;
				if (testptr == 0)
					topBackgroundImage.Image = theme.GetTopAlphaTexture1();
				if (testptr == 1)
					topBackgroundImage.Image = theme.GetTopAlphaTexture2();
				testptr++;

			}

			if (theme.header.bottomScreenFrameType == 2)
			{
				if (frameptr == 3)
					frameptr = 0;

				if (frameptr == 0)
					simBottomBackgroundImage.Image = bottomFrame0;

				if (frameptr == 1)
					simBottomBackgroundImage.Image = bottomFrame1;

				if (frameptr == 2)
					simBottomBackgroundImage.Image = bottomFrame2;
			}
			else if (theme.header.bottomScreenFrameType == 4)
			{
				if (frameptr == 4)
					frameptr = 0;

				if (frameptr == 0)
					simBottomBackgroundImage.Image = bottomFrame0;

				if (frameptr == 1)
					simBottomBackgroundImage.Image = bottomFrame1;

				if (frameptr == 2)
					simBottomBackgroundImage.Image = bottomFrame2;

				if (frameptr == 3)
					simBottomBackgroundImage.Image = bottomFrame1;
			}


			frameptr++;
		}



        private void bitmapFromPngSelect(ref byte[] texturedata, Textures.ImageFormat imageFormat, int fullWidth, int fullHeight, int displayWidth, int displayHeight)
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
                loadImages();
            }
        }

        private void folderOpenImage_Click(object sender, EventArgs e)
        {
			if (theme.header.useFolderTextures)
		        bitmapFromPngSelect(ref theme.openFolderTexture, Textures.ImageFormat.RGB8, theme.folderWidth, theme.folderHeight, 82, 64);			
        }
        private void folderClosedImage_Click(object sender, EventArgs e)
        {
			if (theme.header.useFolderTextures)
	            bitmapFromPngSelect(ref theme.closedFolderTexture, Textures.ImageFormat.RGB8, theme.folderWidth, theme.folderHeight, 74, 64);		
        }
        private void topBackgroundImage_Click(object sender, EventArgs e)
        {
            bitmapFromPngSelect(ref theme.topScreenTexture, Textures.ImageFormat.RGB565, theme.topWidth, theme.topHeight, theme.topClampWidth, theme.topClampHeight);
        }

        private void bottomBackgroundImage_Click(object sender, EventArgs e)
        {
            bitmapFromPngSelect(ref theme.bottomScreenTexture, Textures.ImageFormat.RGB565, theme.bottomWidth, theme.bottomHeight, theme.bottomClampWidth, theme.bottomClampHeight);
        }

        private void iconBorder48pxImage_Click(object sender, EventArgs e)
        {
			if (theme.header.useIconBorders)
	            bitmapFromPngSelect(ref theme.iconBorder48pxTexture, Textures.ImageFormat.RGB8, theme.iconBorder48pxWidth, theme.iconBorder48pxHeight, 36, 72);
        }

        private void iconBorder24pxImage_Click(object sender, EventArgs e)
        {
			if (theme.header.useIconBorders)
	            bitmapFromPngSelect(ref theme.iconBorder24pxTexture, Textures.ImageFormat.RGB8, theme.iconBorder24pxWidth, theme.iconBorder24pxHeight, 25, 50);
        }
    
        private void clampTextureSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            loadImages();
        }

        private void backgroundMusicCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            theme.header.backgroundMusicEnabled = backgroundMusicCheckbox.Checked;
        }

    }
}
