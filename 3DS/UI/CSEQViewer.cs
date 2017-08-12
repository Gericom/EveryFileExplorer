using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3DS.NintendoWare.SND;

namespace _3DS.UI
{
    public partial class CSEQViewer : Form
    {
        CSEQ Sequence;

        public CSEQViewer(CSEQ Sequence)
        {
            this.Sequence = Sequence;
            InitializeComponent();
        }

        public CSEQViewer()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {

        }
    }
}
