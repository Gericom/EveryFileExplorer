using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CommonFiles.UI
{
	public partial class WAVEPlayer : Form
	{
		WAV Wave;
		public WAVEPlayer(WAV Wave)
		{
			this.Wave = Wave;
			InitializeComponent();
		}
	}
}
