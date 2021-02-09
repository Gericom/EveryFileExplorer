using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NDS.UI
{
    public partial class BMGViewer : Form
    {
        BMG Strings;
        public BMGViewer(BMG Strings)
        {
            this.Strings = Strings;
            InitializeComponent();
        }

        public BMGViewer()
        {
            InitializeComponent();
        }
    }
}
