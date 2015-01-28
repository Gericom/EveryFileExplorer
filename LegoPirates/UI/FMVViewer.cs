using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using NDS.SND;
using LibEveryFileExplorer.IO;

namespace LegoPirates.UI
{
	public partial class FMVViewer : Form
	{
		NAudio.Wave.BufferedWaveProvider AudioBuffer;
		NAudio.Wave.WaveOut Player;
		FMV Video;
		public FMVViewer(FMV Video)
		{
			this.Video = Video;
			InitializeComponent();
		}

		private void FMV_Load(object sender, EventArgs e)
		{
			/*double ticks = 10000000.0 / (Video.Header.FrameRate / 256.0);
			float exp = (float)(ticks - Math.Floor(ticks));
			if (exp != 0)
			{
				int i = 0;
				float result;
				do
				{
					i++;
					result = exp * i;
				}
				while((float)(result - Math.Floor(result)) != 0);
			}*/
			//TODO: Calculate timing based on fps
			if ((Video.Header.Flags & 4) == 4)
			{
				AudioConverter = new IMAADPCMDecoder();
				AudioBuffer = new NAudio.Wave.BufferedWaveProvider(new NAudio.Wave.WaveFormat((int)Video.Header.AudioRate, 16, 1));
				AudioBuffer.DiscardOnBufferOverflow = true;
				AudioBuffer.BufferLength = 8192 * 16;
				Player = new NAudio.Wave.WaveOut();
				Player.DesiredLatency = 150;
				Player.Init(AudioBuffer);
				Player.Play();
			}
			new System.Threading.Thread(new System.Threading.ThreadStart(delegate
				{
					int state = 0;
					while (!stop)
					{
						if (Frames.Count != 0)
						{
							pictureBox1.Image = Frames.Dequeue();
							switch (state) 
							{
								case 0: System.Threading.Thread.Sleep(TimeSpan.FromTicks(666666)); break;
								case 1: System.Threading.Thread.Sleep(TimeSpan.FromTicks(666667)); break;
								case 2: System.Threading.Thread.Sleep(TimeSpan.FromTicks(666667)); break;
							}
							state = (state + 1) % 3;
						}
					}
					System.Threading.Thread.CurrentThread.Abort();
				})).Start();
			backgroundWorker1.RunWorkerAsync();
		}

		IMAADPCMDecoder AudioConverter = null;

		int aa = 0;
		int bb = 0;
		bool first = true;

		Queue<Bitmap> Frames = new Queue<Bitmap>();

		private void FMV_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!stop)
			{
				stop = true;
				if ((Video.Header.Flags & 4) == 4)
				{
					Player.Stop();
					Player.Dispose();
					Player = null;
					AudioBuffer = null;
				}
			}
			Video.Close();
		}
		bool stop = false;
		bool stopped = false;
		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			while (!stop)
			{
				if (Frames.Count < 8 || AudioBuffer.BufferedBytes < 8192 * 4)
				{
				retry:
					byte[] audio;
					Bitmap b = Video.GetNextFrame(out audio);
					if (audio != null)
					{
						short[] data = AudioConverter.GetWaveData(audio, 0, audio.Length);
						byte[] result = new byte[data.Length * 2];
						IOUtil.WriteS16sLE(result, 0, data);
						AudioBuffer.AddSamples(result, 0, result.Length);
						goto retry;
					}
					if (b == null)
					{
						stop = true;
						if ((Video.Header.Flags & 4) == 4)
						{
							Player.Stop();
							Player.Dispose();
							Player = null;
							AudioBuffer = null;
						}
					}
					else Frames.Enqueue(b);
				}
			}
		}
	}
}
