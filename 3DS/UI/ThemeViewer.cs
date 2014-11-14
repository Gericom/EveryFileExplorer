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
            widthBox.Text = theme.bottomWidth + "";
            heightBox.Text = theme.bottomHeight + "";
        }

        private void ThemeViewer_Load(object sender, EventArgs e)
        {
            topTextureImage.Image = theme.GetTopTexture();
            bottomTextureImage.Image = theme.GetBottomTexture();
        }


        private void button1_Click(object sender, EventArgs e)
        {

            theme.bottomWidth = int.Parse(widthBox.Text);
            bottomTextureImage.Image = theme.GetBottomTexture();
        }

        private void adjustHeightbutton(object sender, EventArgs e)
        {

            theme.bottomHeight = int.Parse(heightBox.Text);
            bottomTextureImage.Image = theme.GetBottomTexture();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            theme.bottomWidth += 40;
            widthBox.Text = theme.bottomWidth + "";
            bottomTextureImage.Image = theme.GetBottomTexture();
        }

        private void plus40heighbutton_Click(object sender, EventArgs e)
        {

            theme.bottomHeight += 40;
            heightBox.Text = theme.bottomHeight + "";
            bottomTextureImage.Image = theme.GetBottomTexture();
        }


    }
}
