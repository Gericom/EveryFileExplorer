using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WiiU.NintendoWare.LYT2;
using _3DS.NintendoWare.LYT1;

namespace WiiU.UI
{
    public partial class FLYTViewer : Form
    {
        bool init = false;
        FLYT NWLayout;
        FLIM[] Textures;

        public FLYTViewer(FLYT Layout)
        {
            this.NWLayout = Layout;
            InitializeComponent();
        }
    }
}
