using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace MarioKart.UI
{
    public partial class KCLCollisionTypeSelector : Form
    {
        public Dictionary<string, ushort> Mapping;
        public Dictionary<string, bool> Colli;
		public KCLCollisionTypeSelector(string[] names)
        {
            InitializeComponent();
            Mapping = new Dictionary<string, ushort>();
            Colli = new Dictionary<string, bool>();
            for (int i = 0; i < names.Length; i++)
            {
                Mapping.Add(names[i], 0);
                Colli.Add(names[i], true);
                dataGridView1.Rows.Add(names[i], true, "0000");
            }
            if (Mapping.Count == 0)
            {
                Mapping.Add("Default", 0);
                Colli.Add("Default", true);
                dataGridView1.Rows.Add("Default", true, "0000");
            }
        }

        private void kclType_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            ushort hex;
            switch (e.ColumnIndex)
            {
                case 2:
                    e.Cancel = !ushort.TryParse(e.FormattedValue.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hex);
                    break;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 1:
                    Colli[dataGridView1.Rows[e.RowIndex].Cells[0].Value as string] = (bool)dataGridView1.Rows[e.RowIndex].Cells[1].Value;
                    break;
                case 2:
                    dataGridView1.Rows[e.RowIndex].Cells[2].Value = string.Format("{0:X4}", ushort.Parse(dataGridView1.Rows[e.RowIndex].Cells[2].Value as string, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    Mapping[dataGridView1.Rows[e.RowIndex].Cells[0].Value as string] = ushort.Parse(dataGridView1.Rows[e.RowIndex].Cells[2].Value as string, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    break;
            }
        }

        private void kclType_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void kclType_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataGridView1.EndEdit();
        }
    }
}
