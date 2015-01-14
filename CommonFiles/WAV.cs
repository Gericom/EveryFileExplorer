using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;

namespace CommonFiles
{
	public class WAV : FileFormat<WAV.WAVIdentifier>, IWriteable
	{
		/// <summary>
		/// Generates a WAV file with the given <paramref name="Data"/> and params.
		/// </summary>
		/// <param name="Data">The raw sample Data.</param>
		/// <param name="SampleRate">The SampleRate in Hz.</param>
		/// <param name="BitsPerSample">The number of Bits per Sample.</param>
		/// <param name="NrChannel">The number of Channels.</param>
		public WAV(byte[] Data, UInt32 SampleRate, UInt16 BitsPerSample, UInt16 NrChannel)
		{
			Header = new RIFFHeader((uint)(4 + 8 + 16 + 8 + Data.Length + 8));
			Wave = new WaveData(Data, SampleRate, BitsPerSample, NrChannel);
		}

		public WAV(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new RIFFHeader(er);
				Wave = new WaveData(er);
			}
			finally
			{
				er.Close();
			}
		}

		public string GetSaveDefaultFileFilter()
		{
			return "Wave Audio File (*.wav)|*.wav";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.Write(er);
			Wave.Write(er);
			byte[] b = m.ToArray();
			er.Close();
			return b;
		}

		public RIFFHeader Header;
		public class RIFFHeader
		{
			/// <summary>
			/// Creates a new RIFF Header for the given <paramref name="FileSize"/>.
			/// </summary>
			/// <param name="FileSize">Size of the complete file.</param>
			public RIFFHeader(UInt32 FileSize)
			{
				Signature = "RIFF";
				this.FileSize = FileSize - 8;
			}
			public RIFFHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "RIFF") throw new SignatureNotCorrectException(Signature, "RIFF", er.BaseStream.Position - 4);
				FileSize = er.ReadUInt32();
			}
			public String Signature;
			public UInt32 FileSize;//-8

			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(FileSize);
			}
		}
		public WaveData Wave;
		public class WaveData
		{
			public WaveData(byte[] Data, UInt32 SampleRate, UInt16 BitsPerSample, UInt16 NrChannel)
			{
				Signature = "WAVE";
				FMT = new FMTBlock(SampleRate, BitsPerSample, NrChannel);
				DATA = new DATABlock(Data);
			}

			public WaveData(EndianBinaryReader er)
			{
				Signature = er.ReadString(ASCIIEncoding.ASCII, 4);
				if (Signature != "WAVE") throw new SignatureNotCorrectException(Signature, "WAVE", er.BaseStream.Position - 4);
				FMT = new FMTBlock(er);
				String sig = er.ReadString(Encoding.ASCII, 4);
				uint length = er.ReadUInt32();
				er.BaseStream.Position -= 8;
				if (sig == "LIST") er.BaseStream.Position += length + 8;
				DATA = new DATABlock(er);
			}

			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				FMT.Write(er);
				DATA.Write(er);
			}

			public String Signature;
			public FMTBlock FMT;
			public class FMTBlock
			{
				public FMTBlock(UInt32 SampleRate, UInt16 BitsPerSample, UInt16 NrChannel)
				{
					Signature = "fmt ";
					SectionSize = 16;
					AudioFormat = WaveFormat.WAVE_FORMAT_PCM;
					this.NrChannel = NrChannel;
					this.SampleRate = SampleRate;
					this.BitsPerSample = BitsPerSample;
					this.ByteRate = SampleRate * BitsPerSample * NrChannel / 8;
					this.BlockAlign = (UInt16)(NrChannel * BitsPerSample / 8);
				}
				public FMTBlock(EndianBinaryReader er)
				{
					Signature = er.ReadString(Encoding.ASCII, 4);
					if (Signature != "fmt ") throw new SignatureNotCorrectException(Signature, "fmt ", er.BaseStream.Position - 4);
					SectionSize = er.ReadUInt32();
					AudioFormat = (WaveFormat)er.ReadUInt16();
					NrChannel = er.ReadUInt16();
					SampleRate = er.ReadUInt32();
					ByteRate = er.ReadUInt32();
					BlockAlign = er.ReadUInt16();
					BitsPerSample = er.ReadUInt16();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Signature, Encoding.ASCII, false);
					er.Write(SectionSize);
					er.Write((ushort)AudioFormat);
					er.Write(NrChannel);
					er.Write(SampleRate);
					er.Write(ByteRate);
					er.Write(BlockAlign);
					er.Write(BitsPerSample);
				}
				public String Signature;
				public UInt32 SectionSize;
				public WaveFormat AudioFormat;
				public enum WaveFormat : ushort
				{
					WAVE_FORMAT_PCM = 0x0001,
					IBM_FORMAT_ADPCM = 0x0002,
					IBM_FORMAT_MULAW = 0x0007,
					IBM_FORMAT_ALAW = 0x0006,
					WAVE_FORMAT_EXTENSIBLE = 0xFFFE
				}
				public UInt16 NrChannel;
				public UInt32 SampleRate;
				public UInt32 ByteRate;
				public UInt16 BlockAlign;
				public UInt16 BitsPerSample;
			}
			public DATABlock DATA;
			public class DATABlock
			{
				public DATABlock(byte[] Data)
				{
					Signature = "data";
					SectionSize = (UInt32)Data.Length;
					this.Data = Data;
				}

				public DATABlock(EndianBinaryReader er)
				{
					Signature = er.ReadString(Encoding.ASCII, 4);
					if (Signature != "data") throw new SignatureNotCorrectException(Signature, "data", er.BaseStream.Position - 4);
					SectionSize = er.ReadUInt32();
					Data = er.ReadBytes((int)SectionSize);
				}

				public void Write(EndianBinaryWriter er)
				{
					er.Write(Signature, Encoding.ASCII, false);
					er.Write(SectionSize);
					er.Write(Data, 0, Data.Length);
				}
				public String Signature;
				public UInt32 SectionSize;
				public byte[] Data;
			}
		}

		public byte[] GetChannelData(int Channel)
		{
			byte[] result = new byte[Wave.DATA.Data.Length / Wave.FMT.NrChannel];
			int offs = 0;
			for (int i = 0; i < Wave.DATA.Data.Length; i += Wave.FMT.NrChannel * Wave.FMT.BitsPerSample / 8)
			{
				for (int j = 0; j < Wave.FMT.BitsPerSample / 8; j++)
				{
					result[offs++] = Wave.DATA.Data[i + Channel * Wave.FMT.BitsPerSample / 8 + j];
				}
			}
			return result;
		}

		public class WAVIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Audio;
			}

			public override string GetFileDescription()
			{
				return "Wave Audio File (WAV)";
			}

			public override string GetFileFilter()
			{
				return "Wave Audio File (*.wav)|*.wav";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 16 && File.Data[0] == 'R' && File.Data[1] == 'I' && File.Data[2] == 'F' && File.Data[3] == 'F' && File.Data[8] == 'W' && File.Data[9] == 'A' && File.Data[10] == 'V' && File.Data[11] == 'E') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
