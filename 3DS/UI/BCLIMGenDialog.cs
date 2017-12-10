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
    public partial class BCLIMGenDialog : Form
    {
        public BCLIMGenDialog()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 9;
        }
        public int index { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            index = comboBox1.SelectedIndex;
        }
    }
}