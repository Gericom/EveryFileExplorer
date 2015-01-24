using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using _3DS.UI;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace _3DS
{
	public class SMDH : FileFormat<SMDH.SMDHIdentifier>, IViewable, IWriteable
	{
		public SMDH(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new SMDHHeader(er);
				AppTitles = new ApplicationTitle[16];
				for (int i = 0; i < 16; i++) AppTitles[i] = new ApplicationTitle(er);
				AppSettings = new ApplicationSettings(er);
				Reserved = er.ReadBytes(8);
				SmallIcon = er.ReadBytes(0x480);
				LargeIcon = er.ReadBytes(0x1200);
			}
			finally
			{
				er.Close();
			}
		}

		public string GetSaveDefaultFileFilter()
		{
			return "System Menu Data Header (*.icn)|*.icn";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.Write(er);
			for (int i = 0; i < 16; i++) AppTitles[i].Write(er);
			AppSettings.Write(er);
			er.Write(Reserved, 0, 8);
			er.Write(SmallIcon, 0, 0x480);
			er.Write(LargeIcon, 0, 0x1200);
			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public System.Windows.Forms.Form GetDialog()
		{
			return new SMDHViewer(this);
		}

		public SMDHHeader Header;
		public class SMDHHeader
		{
			public SMDHHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				//Signature = er.ReadString(Encoding.ASCII, 4);
				//if (Signature != "SMDH") throw new SignatureNotCorrectException(Signature, "SMDH", er.BaseStream.Position - 4);
				//Version = er.ReadUInt16();
				//Reserved = er.ReadUInt16();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Version);
				er.Write(Reserved);
			}
			[BinaryStringSignature("SMDH")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt16 Version;
			public UInt16 Reserved;
		}

		public ApplicationTitle[] AppTitles;//16
		public class ApplicationTitle
		{
			public ApplicationTitle(EndianBinaryReader er)
			{
				ShortDescription = er.ReadString(Encoding.Unicode, 64).TrimEnd('\0');
				LongDescription = er.ReadString(Encoding.Unicode, 128).TrimEnd('\0');
				Publisher = er.ReadString(Encoding.Unicode, 64).TrimEnd('\0');
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(ShortDescription.PadRight(64, '\0'), Encoding.Unicode, false);
				er.Write(LongDescription.PadRight(128, '\0'), Encoding.Unicode, false);
				er.Write(Publisher.PadRight(64, '\0'), Encoding.Unicode, false);
			}
			public String ShortDescription;//0x80
			public String LongDescription;//0x100
			public String Publisher;//0x80
		}

		public ApplicationSettings AppSettings;
		public class ApplicationSettings
		{
			[Flags]
			public enum RegionLockoutFlags : uint
			{
				Japan = 0x01,
				NorthAmerica = 0x02,
				Europe = 0x04,
				Australia = 0x08,
				China = 0x10,
				Korea = 0x20,
				Taiwan = 0x40
			}

			[Flags]
			public enum AppSettingsFlags : uint
			{
				Visible = 1,
				AutoBoot = 2,
				Allow3D = 4,
				ReqAcceptEULA = 8,
				AutoSaveOnExit = 16,
				UsesExtendedBanner = 32,
				ReqRegionGameRating = 64,
				UsesSaveData = 128,
				RecordUsage = 256,
				DisableSDSaveBackup = 512
			}

			public ApplicationSettings(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				/*GameRatings = er.ReadBytes(0x10);
				RegionLockout = (RegionLockoutFlags)er.ReadUInt32();
				MatchMakerID = er.ReadUInt32();
				MatchMakerBITID = er.ReadUInt64();
				Flags = (AppSettingsFlags)er.ReadUInt32();
				EULAVersion = er.ReadUInt16();
				Reserved = er.ReadUInt16();
				AnimationDefaultFrame = er.ReadSingle();
				StreetPassID = er.ReadUInt32();*/
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(GameRatings, 0, 0x10);
				er.Write((uint)RegionLockout);
				er.Write(MatchMakerID);
				er.Write(MatchMakerBITID);
				er.Write((uint)Flags);
				er.Write(EULAVersion);
				er.Write(Reserved);
				er.Write(AnimationDefaultFrame);
				er.Write(StreetPassID);
			}
			[BinaryFixedSize(0x10)]
			public byte[] GameRatings;//0x10
			public RegionLockoutFlags RegionLockout;
			public UInt32 MatchMakerID;
			public UInt64 MatchMakerBITID;
			public AppSettingsFlags Flags;
			public UInt16 EULAVersion;
			public UInt16 Reserved;
			public Single AnimationDefaultFrame;
			public UInt32 StreetPassID;
		}

		public byte[] Reserved;//8

		public byte[] SmallIcon;//0x480
		public byte[] LargeIcon;//0x1200

		public Bitmap GetSmallIcon()
		{
			return GPU.Textures.ToBitmap(SmallIcon, 24, 24, GPU.Textures.ImageFormat.RGB565, true);
		}

		public Bitmap GetLargeIcon()
		{
			return GPU.Textures.ToBitmap(LargeIcon, 48, 48, GPU.Textures.ImageFormat.RGB565, true);
		}

		public class SMDHIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "3DS";
			}

			public override string GetFileDescription()
			{
				return "System Menu Data Header (SMDH)";
			}

			public override string GetFileFilter()
			{
				return "System Menu Data Header (*.icn, icon.bin)|*.icn;icon.bin";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'M' && File.Data[2] == 'D' && File.Data[3] == 'H') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
