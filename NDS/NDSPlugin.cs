using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer;
using LibEveryFileExplorer.Script;
using System.IO;
using CommonFiles;
using LibEveryFileExplorer.IO;

namespace NDS
{
	public class NDSPlugin : EFEPlugin
	{
		public override void OnLoad()
		{
			WAV w = new WAV(File.ReadAllBytes(@"d:\Temp\MKDSMission\Music\Poo_Intro.wav"));
			var chan0 = w.GetChannelData(0);
			Int16[] samples = IOUtil.ReadS16sLE(chan0, 0, chan0.Length / 2);
			var dat = SND.ADPCM.Encode(samples);
			Int16[] dec = new SND.ADPCM().GetWaveData(dat, 0, dat.Length);
			byte[] decdata = new byte[dec.Length * 2];
			IOUtil.WriteS16sLE(decdata, 0, dec);
			WAV w2 = new WAV(decdata, w.Wave.FMT.SampleRate, 16, 1);
			File.Create(@"d:\Temp\EFE\ADPCM\test2.wav").Close();
			File.WriteAllBytes(@"d:\Temp\EFE\ADPCM\test2.wav", w2.Write());


			WAV ww = new WAV(chan0, w.Wave.FMT.SampleRate, 16, 1);
			File.Create(@"d:\Temp\EFE\ADPCM\original.wav").Close();
			File.WriteAllBytes(@"d:\Temp\EFE\ADPCM\original.wav", ww.Write());
		}
	}
}
