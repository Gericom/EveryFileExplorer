using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using System.IO;
using LibEveryFileExplorer.SND;
using LibEveryFileExplorer.IO;

namespace CommonFiles.UI
{
	public partial class WavePlayer : UserControl
	{
		private WaveOut Output;
		private WaveStream Stream;

		private bool IsPlaying = false;

		public WavePlayer()
		{
			InitializeComponent();
			Output = new WaveOut();
			Output.NumberOfBuffers = 10;
		}

		public void SetWavFile(WAV Wave)
		{
			if (Wave.Wave.FMT.NrChannel > 2)
			{
				byte[] left = Wave.GetChannelData(0);
				byte[] right = Wave.GetChannelData(1);
				WAV wav2 = new WAV(SNDUtil.InterleaveChannels(IOUtil.ReadS16sLE(left, 0, left.Length / 2), IOUtil.ReadS16sLE(right, 0, right.Length / 2)), Wave.Wave.FMT.SampleRate, 16, 2);
				Stream = new WaveFileReader(new MemoryStream(wav2.Write()));
			}
			else
			{
				Stream = new WaveFileReader(new MemoryStream(Wave.Write()));
			}
			Output.Init(Stream);
			trackBar1.Maximum = (int)Math.Ceiling(Stream.TotalTime.TotalSeconds);
		}

		public void Stop()
		{
			Output.Stop();
			Stream.Close();
			Stream.Dispose();
			Stream = null;
			Output = new WaveOut();
			timer1.Stop();
			timer1.Enabled = false;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (IsPlaying)
			{
				button1.Image = Resource.control;
				IsPlaying = false;
				Output.Pause();
				timer1.Stop();
			}
			else if (Output.PlaybackState == PlaybackState.Paused)
			{
				button1.Image = Resource.control_pause;
				Output.Resume();
				IsPlaying = true;
				timer1.Enabled = true;
				timer1.Start();
			}
			else
			{
				button1.Image = Resource.control_pause;
				Output.Play();
				IsPlaying = true;
				timer1.Enabled = true;
				timer1.Start();
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			trackBar1.Value = (int)Math.Ceiling(Stream.CurrentTime.TotalSeconds);
			if (Stream.CurrentTime == Stream.TotalTime)
			{
				Output.Stop();
				trackBar1.Value = 0;
				Stream.Seek(0, SeekOrigin.Begin);
				IsPlaying = false;
				timer1.Stop();
				button1.Image = Resource.control;
			}
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			Stream.CurrentTime = TimeSpan.FromSeconds(trackBar1.Value);
		}
	}
}
