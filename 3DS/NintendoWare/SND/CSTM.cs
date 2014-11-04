using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using _3DS.DSP;
using LibEveryFileExplorer.SND;
using CommonFiles;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.SND
{
	public class CSTM : FileFormat<CSTM.CSTMIdentifier>, IConvertable, IFileCreatable, IViewable, IWriteable
	{
		public CSTM()
		{
			Header = new CSTMHeader(false);
			Info = new INFO(true);
			Data = new DATA();
		}
		public CSTM(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CSTMHeader(er);
				er.BaseStream.Position = Header.Sections[0].Offset;
				Info = new INFO(er);
				if (Header.NrSections > 2 && Header.Sections[1].Id == 0x4001)
				{
					er.BaseStream.Position = Header.Sections[1].Offset;
					Seek = new SEEK(er);
					er.BaseStream.Position = Header.Sections[2].Offset;
					this.Data = new DATA(er);
				}
				else
				{
					er.BaseStream.Position = Header.Sections[1].Offset;
					this.Data = new DATA(er);
				}
			}
			finally
			{
				er.Close();
			}
		}

		public string GetSaveDefaultFileFilter()
		{
			return "CTR Stream (*.bcstm)|*.bcstm";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.Write(er);
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = 0x18;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			Info.Write(er);

			long length = er.BaseStream.Position - curpos;
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 0x1C;
			er.Write((uint)length);
			er.BaseStream.Position = 0x24;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			if (Seek != null)
			{
				Seek.Write(er);
				length = er.BaseStream.Position - curpos;
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = 0x28;
				er.Write((uint)length);
				er.BaseStream.Position = 0x30;
				er.Write((uint)curpos);
				er.BaseStream.Position = curpos;
				Data.Write(er);
				length = er.BaseStream.Position - curpos;
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = 0x34;
				er.Write((uint)length);
				er.BaseStream.Position = curpos;
			}
			else
			{
				Data.Write(er);
				length = er.BaseStream.Position - curpos;
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = 0x28;
				er.Write((uint)length);
				er.BaseStream.Position = curpos;
			}
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 0xC;
			er.Write((uint)curpos);
			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public System.Windows.Forms.Form GetDialog()
		{
			return new CommonFiles.UI.WAVEPlayer(ToWave());
		}

		public string GetConversionFileFilters()
		{
			return "Wave File (*.wav)|*.wav";
		}

		public bool Convert(int FilterIndex, String Path)
		{
			switch (FilterIndex)
			{
				case 0:
					byte[] Data = ToWave().Write();
					File.Create(Path).Close();
					File.WriteAllBytes(Path, Data);
					return true;
				default:
					return false;
			}
		}

		public bool CreateFromFile()
		{
			System.Windows.Forms.OpenFileDialog f = new System.Windows.Forms.OpenFileDialog();
			f.Filter = WAV.Identifier.GetFileFilter();
			if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& f.FileName.Length > 0)
			{
				WAV w = new WAV(File.ReadAllBytes(f.FileName));
				Header = new CSTMHeader(false);
				Info = new INFO(true);
				Data = new DATA();
				Info.StreamInfo.SampleRate = w.Wave.FMT.SampleRate;
				Info.StreamInfo.NrChannels = (byte)w.Wave.FMT.NrChannel;
				if (w.Wave.FMT.AudioFormat == WAV.WaveData.FMTBlock.WaveFormat.WAVE_FORMAT_PCM && w.Wave.FMT.BitsPerSample == 16)
				{
					Info.StreamInfo.Format = 1;
					Info.StreamInfo.SeekInterval = 0x1000;
					Info.StreamInfo.LoopEnd = (uint)((w.Wave.DATA.Data.Length / w.Wave.FMT.NrChannel) / 2);
					Info.StreamInfo.BlockSize = 0x2000;
					Info.StreamInfo.BlockNrSamples = 0x2000 / 2;
					Info.StreamInfo.NrBlocks = (uint)((w.Wave.DATA.Data.Length / w.Wave.FMT.NrChannel) / 0x2000) + 1;
					Info.StreamInfo.LastBlockSize = (uint)((w.Wave.DATA.Data.Length / w.Wave.FMT.NrChannel) % 0x2000);
					Info.StreamInfo.LastBlockPaddedSize = Info.StreamInfo.LastBlockSize;
					while ((Info.StreamInfo.LastBlockPaddedSize % 4) != 0) Info.StreamInfo.LastBlockPaddedSize++;
					Info.StreamInfo.LastBlockNrSamples = Info.StreamInfo.LastBlockSize / 2;
					Info.TrackInfoReferenceTable.NrEntries = 1;
					Info.TrackInfoReferenceTable.Entries = new INFO.SectionInfo[] { new INFO.SectionInfo(0x4101) };
					Info.TrackInfos = new INFO.TrackInfo[] { new INFO.TrackInfo() };
					Info.ChannelInfoReferenceTable.NrEntries = 2;
					Info.ChannelInfoReferenceTable.Entries = new INFO.SectionInfo[] { new INFO.SectionInfo(0x4102), new INFO.SectionInfo(0x4102) };
					Info.ChannelInfos = new INFO.ChannelInfo[] { new INFO.ChannelInfo(), new INFO.ChannelInfo() };
					byte[][] channels = new byte[w.Wave.FMT.NrChannel][];
					for (int i = 0; i < w.Wave.FMT.NrChannel; i++)
					{
						channels[i] = w.GetChannelData(i);
					}
					Data.Data = new byte[(Info.StreamInfo.NrBlocks - 1) * 0x2000 * w.Wave.FMT.NrChannel + Info.StreamInfo.LastBlockPaddedSize * w.Wave.FMT.NrChannel];
					int offs = 0;
					for (int i = 0; i < Info.StreamInfo.NrBlocks - 1; i++)
					{
						for (int j = 0; j < w.Wave.FMT.NrChannel; j++)
						{
							Array.Copy(channels[j], i * 0x2000, Data.Data, offs, 0x2000);
							offs += 0x2000;
						}
					}
					for (int j = 0; j < w.Wave.FMT.NrChannel; j++)
					{
						Array.Copy(channels[j], channels[j].Length - Info.StreamInfo.LastBlockSize, Data.Data, offs, Info.StreamInfo.LastBlockSize);
						offs += (int)Info.StreamInfo.LastBlockSize;
						while ((offs % 4) != 0) offs++;
					}
					return true;
				}
				else return false;
			}
			return false;
		}

		public CSTMHeader Header;
		public class CSTMHeader
		{
			public CSTMHeader(bool HasSeek)
			{
				Signature = "CSTM";
				Endianness = 0xFEFF;
				HeaderSize = 0x40;
				Version = 0x02000000;
				NrSections = (uint)(HasSeek ? 3 : 2);
				if (HasSeek) Sections = new SectionInfo[] { new SectionInfo(0x4000), new SectionInfo(0x4001), new SectionInfo(0x4002) };
				else Sections = new SectionInfo[] { new SectionInfo(0x4000), new SectionInfo(0x4002) };
			}
			public CSTMHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CSTM") throw new SignatureNotCorrectException(Signature, "CSTM", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				FileSize = er.ReadUInt32();
				NrSections = er.ReadUInt32();
				Sections = new SectionInfo[NrSections];
				for (int i = 0; i < NrSections; i++) Sections[i] = new SectionInfo(er);
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Endianness);
				er.Write(HeaderSize);
				er.Write(Version);
				er.Write((uint)0);
				er.Write(NrSections);
				foreach (var v in Sections) v.Write(er);
				while ((er.BaseStream.Position % 32) != 0) er.Write((byte)0);
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 FileSize;
			public UInt32 NrSections;
			public SectionInfo[] Sections;
			public class SectionInfo
			{
				public SectionInfo(uint Id) { this.Id = Id; }
				public SectionInfo(EndianBinaryReader er)
				{
					Id = er.ReadUInt32();
					Offset = er.ReadUInt32();
					Size = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Id);
					er.Write(Offset);
					er.Write(Size);
				}
				public UInt32 Id;//0x4000 = INFO, 0x4001 = SEEK, 0x4002 = DATA
				public UInt32 Offset;
				public UInt32 Size;
			}
		}

		public INFO Info;
		public class INFO
		{
			public INFO(bool HasTracks)
			{
				Signature = "INFO";
				StreamSoundInfoReference = new SectionInfo(0x4100);
				TrackInfoReference = new SectionInfo((uint)(HasTracks ? 0x101 : 0));
				ChannelInfoReference = new SectionInfo(0x101);
				StreamInfo = new StreamSoundInfo();
				if (HasTracks) TrackInfoReferenceTable = new ReferenceTable();
				ChannelInfoReferenceTable = new ReferenceTable();
			}
			public INFO(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "INFO") throw new SignatureNotCorrectException(Signature, "INFO", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				long offs = er.BaseStream.Position;
				StreamSoundInfoReference = new SectionInfo(er);
				TrackInfoReference = new SectionInfo(er);
				ChannelInfoReference = new SectionInfo(er);
				er.BaseStream.Position = offs + StreamSoundInfoReference.Offset;
				StreamInfo = new StreamSoundInfo(er);
				if ((int)TrackInfoReference.Offset != -1)
				{
					er.BaseStream.Position = offs + TrackInfoReference.Offset;
					TrackInfoReferenceTable = new ReferenceTable(er);
				}
				er.BaseStream.Position = offs + ChannelInfoReference.Offset;
				ChannelInfoReferenceTable = new ReferenceTable(er);
				int i;
				if ((int)TrackInfoReference.Offset != -1)
				{
					TrackInfos = new TrackInfo[TrackInfoReferenceTable.NrEntries];
					i = 0;
					foreach (var v in TrackInfoReferenceTable.Entries)
					{
						er.BaseStream.Position = offs + TrackInfoReference.Offset + v.Offset;
						TrackInfos[i] = new TrackInfo(er);
						i++;
					}
				}
				ChannelInfos = new ChannelInfo[ChannelInfoReferenceTable.NrEntries];
				i = 0;
				foreach (var v in ChannelInfoReferenceTable.Entries)
				{
					er.BaseStream.Position = offs + ChannelInfoReference.Offset + v.Offset;
					ChannelInfos[i] = new ChannelInfo(er);
					i++;
				}
				if (StreamInfo.Format == 2)
				{
					CodecInfos = new DSPADPCMCodecInfo[ChannelInfoReferenceTable.NrEntries];
					i = 0;
					foreach (var v in ChannelInfos)
					{
						if ((int)v.CodecInfoOffset == -1) { i++; continue; }
						er.BaseStream.Position = offs + ChannelInfoReference.Offset + ChannelInfoReferenceTable.Entries[i].Offset + v.CodecInfoOffset;
						CodecInfos[i] = new DSPADPCMCodecInfo(er);
						i++;
					}
				}
				else
				{

				}
			}
			public void Write(EndianBinaryWriter er)
			{
				long basepos = er.BaseStream.Position;
				er.Write(Signature, Encoding.ASCII, false);
				er.Write((uint)0);
				StreamSoundInfoReference.Write(er);
				TrackInfoReference.Write(er);
				ChannelInfoReference.Write(er);
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = basepos + 8;
				er.Write((uint)(0x4100));
				er.Write((uint)(curpos - basepos - 8));
				er.BaseStream.Position = curpos;
				StreamInfo.Write(er);
				long trackreftableoffs = -1;
				if (TrackInfoReferenceTable != null)
				{
					trackreftableoffs = curpos = er.BaseStream.Position;
					er.BaseStream.Position = basepos + 16;
					er.Write((uint)(0x101));
					er.Write((uint)(curpos - basepos - 8));
					er.BaseStream.Position = curpos;
					TrackInfoReferenceTable.Write(er);
				}
				else
				{
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = basepos + 16;
					er.Write((uint)(0));
					er.Write((uint)(0xFFFFFFFF));
					er.BaseStream.Position = curpos;
					er.Write((uint)0x100);
					er.Write((uint)0);
					er.Write((uint)0xFFFFFFFF);
				}
				long chanreftableoffs = curpos = er.BaseStream.Position;
				er.BaseStream.Position = basepos + 24;
				er.Write((uint)(0x101));
				er.Write((uint)(curpos - basepos - 8));
				er.BaseStream.Position = curpos;
				ChannelInfoReferenceTable.Write(er);
				if (TrackInfoReferenceTable != null)
				{
					for (int i = 0; i < TrackInfos.Length; i++)
					{
						curpos = er.BaseStream.Position;
						er.BaseStream.Position = trackreftableoffs + 4 + 8 * i;
						er.Write((uint)(0x4101));
						er.Write((uint)(curpos - trackreftableoffs));
						er.BaseStream.Position = curpos;
						TrackInfos[i].Write(er);
					}
				}
				long[] chaninfooffs = new long[ChannelInfos.Length];
				for (int i = 0; i < ChannelInfos.Length; i++)
				{
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = chanreftableoffs + 4 + 8 * i;
					er.Write((uint)(0x4102));
					er.Write((uint)(curpos - chanreftableoffs));
					chaninfooffs[i] = er.BaseStream.Position = curpos;
					ChannelInfos[i].Write(er);
				}
				if (StreamInfo.Format == 2)
				{
					for (int i = 0; i < ChannelInfos.Length; i++)
					{
						curpos = er.BaseStream.Position;
						er.BaseStream.Position = chaninfooffs[i];
						er.Write((uint)(0x300));
						er.Write((uint)(curpos - chaninfooffs[i]));
						er.BaseStream.Position = curpos;
						CodecInfos[i].Write(er);
					}
				}
				while ((er.BaseStream.Position % 32) != 0) er.Write((byte)0);
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = basepos + 4;
				er.Write((uint)(curpos - basepos));
				er.BaseStream.Position = curpos;
			}
			public String Signature;
			public UInt32 SectionSize;
			public SectionInfo StreamSoundInfoReference;
			public class SectionInfo
			{
				public SectionInfo(uint Id) { this.Id = Id; }
				public SectionInfo(EndianBinaryReader er)
				{
					Id = er.ReadUInt32();
					Offset = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Id);
					er.Write(Offset);
				}
				public UInt32 Id;
				public UInt32 Offset;//Relative to start of the whole block, eventually including the number of entries
			}
			public SectionInfo TrackInfoReference;
			public SectionInfo ChannelInfoReference;
			public StreamSoundInfo StreamInfo;
			public class StreamSoundInfo
			{
				public StreamSoundInfo()
				{
					NrChannels = 1;
					SampleRate = 44100;
					BlockSize = 0x2000;
					LastBlockSize = 0x2000;
					SeekEntrySize = 4;
					//SeekInterval = 0x2000;
					ReferenceSignature = 0x1F00;
					DataBlockDataOffset = 0x18;
				}
				public StreamSoundInfo(EndianBinaryReader er)
				{
					Format = er.ReadByte();
					Loop = er.ReadByte() == 1;
					NrChannels = er.ReadByte();
					er.ReadByte();//padding
					SampleRate = er.ReadUInt32();
					LoopStart = er.ReadUInt32();
					LoopEnd = er.ReadUInt32();
					NrBlocks = er.ReadUInt32();
					BlockSize = er.ReadUInt32();
					BlockNrSamples = er.ReadUInt32();
					LastBlockSize = er.ReadUInt32();
					LastBlockNrSamples = er.ReadUInt32();
					LastBlockPaddedSize = er.ReadUInt32();
					SeekEntrySize = er.ReadUInt32();
					SeekInterval = er.ReadUInt32();
					ReferenceSignature = er.ReadUInt32();
					DataBlockDataOffset = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Format);
					er.Write((byte)(Loop ? 1 : 0));
					er.Write(NrChannels);
					er.Write((byte)0);
					er.Write(SampleRate);
					er.Write(LoopStart);
					er.Write(LoopEnd);
					er.Write(NrBlocks);
					er.Write(BlockSize);
					er.Write(BlockNrSamples);
					er.Write(LastBlockSize);
					er.Write(LastBlockNrSamples);
					er.Write(LastBlockPaddedSize);
					er.Write(SeekEntrySize);
					er.Write(SeekInterval);
					er.Write(ReferenceSignature);
					er.Write(DataBlockDataOffset);
				}
				public Byte Format;
				public bool Loop;
				public Byte NrChannels;
				public UInt32 SampleRate;
				public UInt32 LoopStart;
				public UInt32 LoopEnd;
				public UInt32 NrBlocks;
				public UInt32 BlockSize;
				public UInt32 BlockNrSamples;
				public UInt32 LastBlockSize;
				public UInt32 LastBlockNrSamples;
				public UInt32 LastBlockPaddedSize;
				public UInt32 SeekEntrySize;
				public UInt32 SeekInterval;//nr of samples between a seek entry
				public UInt32 ReferenceSignature;
				public UInt32 DataBlockDataOffset;//Including padding
			}
			public class ReferenceTable
			{
				public ReferenceTable() { Entries = new SectionInfo[0]; }
				public ReferenceTable(EndianBinaryReader er)
				{
					NrEntries = er.ReadUInt32();
					Entries = new SectionInfo[NrEntries];
					for (int i = 0; i < NrEntries; i++)
					{
						Entries[i] = new SectionInfo(er);
					}
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(NrEntries);
					for (int i = 0; i < NrEntries; i++)
					{
						Entries[i].Write(er);
					}
				}
				public UInt32 NrEntries;
				public SectionInfo[] Entries;
			}
			public ReferenceTable TrackInfoReferenceTable;
			public TrackInfo[] TrackInfos;
			public class TrackInfo
			{
				public TrackInfo()
				{
					Volume = 0x7F;
					Pan = 0x40;
					ChannelIndexTableSignature = 0x100;
					ChannelIndexTableDataOffset = 0xC;
					ChannelIndexTableNrEntries = 2;
					ChannelIndexTableData = new byte[] { 0, 1 };
				}
				public TrackInfo(EndianBinaryReader er)
				{
					Volume = er.ReadByte();
					Pan = er.ReadByte();
					er.ReadUInt16();//padding
					long curpos = er.BaseStream.Position;
					ChannelIndexTableSignature = er.ReadUInt32();
					ChannelIndexTableDataOffset = er.ReadUInt32();
					ChannelIndexTableNrEntries = er.ReadUInt32();
					er.BaseStream.Position = curpos + ChannelIndexTableDataOffset;
					ChannelIndexTableData = er.ReadBytes((int)ChannelIndexTableNrEntries);
					while ((er.BaseStream.Position % 4) != 0) er.ReadByte();
				}
				public void Write(EndianBinaryWriter er)
				{
					ChannelIndexTableNrEntries = (uint)ChannelIndexTableData.Length;
					er.Write(Volume);
					er.Write(Pan);
					er.Write((ushort)0);
					er.Write((uint)0x100);
					er.Write((uint)0xC);
					er.Write(ChannelIndexTableNrEntries);
					er.Write(ChannelIndexTableData, 0, ChannelIndexTableData.Length);
					while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0);
				}
				public Byte Volume;
				public Byte Pan;
				public UInt32 ChannelIndexTableSignature;//0x100
				public UInt32 ChannelIndexTableDataOffset;
				public UInt32 ChannelIndexTableNrEntries;
				public byte[] ChannelIndexTableData;
			}
			public ReferenceTable ChannelInfoReferenceTable;
			public ChannelInfo[] ChannelInfos;
			public class ChannelInfo
			{
				public ChannelInfo()
				{ 
					CodecInfoOffset = 0xFFFFFFFF;
				}
				public ChannelInfo(EndianBinaryReader er)
				{
					CodecSignature = er.ReadUInt32();
					CodecInfoOffset = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(CodecSignature);
					er.Write(CodecInfoOffset);
				}
				public UInt32 CodecSignature;
				public UInt32 CodecInfoOffset;//Relative to start of this block
			}
			public DSPADPCMCodecInfo[] CodecInfos;
			public class DSPADPCMCodecInfo
			{
				public DSPADPCMCodecInfo(EndianBinaryReader er)
				{
					Table = er.ReadInt16s(16);
					Scale = er.ReadUInt16();
					Last1 = er.ReadInt16();
					Last2 = er.ReadInt16();
					LoopScale = er.ReadUInt16();
					LoopLast1 = er.ReadInt16();
					LoopLast2 = er.ReadInt16();
					Unknown6 = er.ReadUInt16();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Table, 0, 16);
					er.Write(Scale);
					er.Write(Last1);
					er.Write(Last2);
					er.Write(LoopScale);
					er.Write(LoopLast1);
					er.Write(LoopLast2);
					er.Write(Unknown6);
				}
				public Int16[] Table;//[16];
				public UInt16 Scale;
				public Int16 Last1;
				public Int16 Last2;
				public UInt16 LoopScale;
				public Int16 LoopLast1;
				public Int16 LoopLast2;
				public UInt16 Unknown6;
			};
		}

		public SEEK Seek;
		public class SEEK
		{
			public SEEK()
			{
				Signature = "SEEK";
				Samples = new short[0];
			}
			public SEEK(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "SEEK") throw new SignatureNotCorrectException(Signature, "SEEK", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				Samples = er.ReadInt16s((int)((SectionSize - 8) / 2));
			}
			public void Write(EndianBinaryWriter er)
			{
				SectionSize = (uint)(Samples.Length * 2 + 8);
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(SectionSize);
				er.Write(Samples, 0, Samples.Length);
				while ((er.BaseStream.Position % 32) != 0) er.Write((byte)0);
			}
			public String Signature;
			public UInt32 SectionSize;
			public Int16[] Samples;
		}

		public DATA Data;
		public class DATA
		{
			public DATA()
			{
				Signature = "DATA";
				Data = new byte[0];
			}
			public DATA(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				er.ReadBytes(0x18);//padding
				Data = er.ReadBytes((int)(SectionSize - 0x20));
			}
			public void Write(EndianBinaryWriter er)
			{
				long basepos = er.BaseStream.Position;
				er.Write(Signature, Encoding.ASCII, false);
				er.Write((uint)0);
				er.Write(new byte[0x18], 0, 0x18);
				er.Write(Data, 0, Data.Length);
				while ((er.BaseStream.Position % 32) != 0) er.Write((byte)0);
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = basepos + 4;
				er.Write((uint)(curpos - basepos));
				er.BaseStream.Position = curpos;
			}
			public String Signature;
			public UInt32 SectionSize;
			public byte[] Data;
		}

		public Int16[] GetChannelData(int Channel)
		{
			if (Channel >= Info.StreamInfo.NrChannels) return null;
			if (Info.StreamInfo.Format == 2)
			{
				ADPCM worker = new ADPCM(Info.CodecInfos[Channel].Table);
				List<short> Data = new List<short>();
				for (int i = 0; i < this.Data.Data.Length; i += (int)Info.StreamInfo.BlockSize * Info.StreamInfo.NrChannels)
				{
					if (i != 0)
					{
						Data[Data.Count - 2] = Seek.Samples[(i / Info.StreamInfo.BlockSize) * 2 + Channel * 2 + 1];
						Data[Data.Count - 1] = Seek.Samples[(i / Info.StreamInfo.BlockSize) * 2 + Channel * 2];
					}
					worker.UpdateLastSamples(Seek.Samples[(i / Info.StreamInfo.BlockSize) * 2 + Channel * 2], Seek.Samples[(i / Info.StreamInfo.BlockSize) * 2 + Channel * 2 + 1]);
					if (i + Info.StreamInfo.BlockSize * Info.StreamInfo.NrChannels < this.Data.Data.Length) Data.AddRange(worker.GetWaveData(this.Data.Data, i + Channel * (int)Info.StreamInfo.BlockSize, (int)Info.StreamInfo.BlockSize));
					else if (Info.StreamInfo.BlockSize != Info.StreamInfo.LastBlockPaddedSize) Data.AddRange(worker.GetWaveData(this.Data.Data, i + Channel * (int)Info.StreamInfo.LastBlockPaddedSize, (int)Info.StreamInfo.LastBlockPaddedSize));
					else break;
				}
				return Data.ToArray();
			}
			else if (Info.StreamInfo.Format == 1)
			{
				List<short> Data = new List<short>();
				for (int i = 0; i < this.Data.Data.Length; i += (int)Info.StreamInfo.BlockSize * Info.StreamInfo.NrChannels)
				{
					if (i + Info.StreamInfo.BlockSize * Info.StreamInfo.NrChannels < this.Data.Data.Length)
					{
						for (int q = 0; q < Info.StreamInfo.BlockSize; q += 2)
						{
							Data.Add(IOUtil.ReadS16LE(this.Data.Data, (int)(i + Channel * Info.StreamInfo.BlockSize + q)));
						}
					}
					else if (Info.StreamInfo.BlockSize != Info.StreamInfo.LastBlockSize)
					{
						for (int q = 0; q < Info.StreamInfo.LastBlockSize; q += 2)
						{
							Data.Add(IOUtil.ReadS16LE(this.Data.Data, (int)(i + Channel * Info.StreamInfo.LastBlockPaddedSize + q)));
						}
					}
					else break;
				}
				return Data.ToArray();
			}
			else
			{
				return new short[0];
			}
		}

		public WAV ToWave()
		{
			Int16[][] Channels = new short[Info.StreamInfo.NrChannels][];
			for (int i = 0; i < Info.StreamInfo.NrChannels; i++)
			{
				Channels[i] = GetChannelData(i);
			}
			return new CommonFiles.WAV(SNDUtil.InterleaveChannels(Channels), Info.StreamInfo.SampleRate, 16, Info.StreamInfo.NrChannels);
		}

		public class CSTMIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Audio;
			}

			public override string GetFileDescription()
			{
				return "CTR Stream (CSTM)";
			}

			public override string GetFileFilter()
			{
				return "CTR Stream (*.bcstm)|*.bcstm";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'S' && File.Data[2] == 'T' && File.Data[3] == 'M') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
