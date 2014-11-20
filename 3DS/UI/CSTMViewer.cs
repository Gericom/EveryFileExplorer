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
	public partial class CSTMViewer : Form
	{
		CSTM Stream;

		public CSTMViewer(CSTM Stream)
		{
			this.Stream = Stream;
			InitializeComponent();
		}

		private void CSTMViewer_FormClosed(object sender, FormClosedEventArgs e)
		{
			wavePlayer1.Stop();
		}

		private void CSTMViewer_Load(object sender, EventArgs e)
		{
			//Not in the constructor, because it will cause problems when creating a bcstm
			wavePlayer1.SetWavFile(Stream.ToWave());
		}
	}
}
