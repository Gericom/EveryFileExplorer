using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.IO.Serialization;
using LibEveryFileExplorer.IO;
using System.IO;
using System.Windows.Forms;

namespace _3DS
{
	public class ThemeNew : FileFormat<ThemeNew.ThemeIdentifier>//, IViewable
	{
		public ThemeNew(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new ThemeHeader(er);
				if (Header.TopBGColorOffset != 0)
				{
					er.BaseStream.Position = Header.TopBGColorOffset;
					TopBGColor = new ThemeTopBGColor(er);
				}
				if (Header.TopBGTexture0Offset != 0)
				{
					er.BaseStream.Position = Header.TopBGTexture0Offset;
					Bitmap b;
					switch (Header.TopBGType)
					{
						case ThemeHeader.BGType.Wallpaper:
							switch (Header.TopBGMoveType)
							{
								case ThemeHeader.BGMoveType.Normal:
								case ThemeHeader.BGMoveType.Floating:
									TopBGTexture0 = er.ReadBytes(1024 * 256 * 2);
									//b = GPU.Textures.ToBitmap(TopBGTexture0, 1008, 240, GPU.Textures.ImageFormat.RGB565);
									break;
								case ThemeHeader.BGMoveType.Fixed:
									TopBGTexture0 = er.ReadBytes(512 * 256 * 2);
									//b = GPU.Textures.ToBitmap(TopBGTexture0, 412, 240, GPU.Textures.ImageFormat.RGB565);
									break;
							}
							break;
						case ThemeHeader.BGType.Pattern:
							TopBGTexture0 = er.ReadBytes(64 * 64);
							//b = GPU.Textures.ToBitmap(TopBGTexture0, 64, 64, GPU.Textures.ImageFormat.L8);
							break;
					}
				}
				if (Header.TopBGTexture1Offset != 0)
				{
					er.BaseStream.Position = Header.TopBGTexture1Offset;
					Bitmap b;
					switch (Header.TopBGType)
					{
						case ThemeHeader.BGType.Wallpaper:
							switch (Header.TopBGMoveType)
							{
								case ThemeHeader.BGMoveType.Normal:
								case ThemeHeader.BGMoveType.Floating:
									TopBGTexture1 = er.ReadBytes(1024 * 256 * 2);
									//b = GPU.Textures.ToBitmap(TopBGTexture1, 1008, 240, GPU.Textures.ImageFormat.RGB565);
									break;
								case ThemeHeader.BGMoveType.Fixed:
									TopBGTexture1 = er.ReadBytes(512 * 256 * 2);
									//b = GPU.Textures.ToBitmap(TopBGTexture1, 412, 240, GPU.Textures.ImageFormat.RGB565);
									break;
							}
							break;
						case ThemeHeader.BGType.Pattern:
							TopBGTexture1 = er.ReadBytes(64 * 64);
							//b = GPU.Textures.ToBitmap(TopBGTexture1, 64, 64, GPU.Textures.ImageFormat.L8);
							break;
						default:
							break;
					}
				}
				if (Header.BottomBGTextureOffset != 0)
				{
					er.BaseStream.Position = Header.BottomBGTextureOffset;
					Bitmap b;
					switch (Header.BottomBGType)
					{
						case ThemeHeader.BGType.Wallpaper:
							switch (Header.BottomBGMoveType)
							{
								case ThemeHeader.BGMoveType.Normal:
								case ThemeHeader.BGMoveType.Floating:
									TopBGTexture0 = er.ReadBytes(1024 * 256 * 2);
									//b = GPU.Textures.ToBitmap(TopBGTexture0, 1008, 240, GPU.Textures.ImageFormat.RGB565);
									break;
								case ThemeHeader.BGMoveType.Fixed:
									TopBGTexture0 = er.ReadBytes(512 * 256 * 2);
									//b = GPU.Textures.ToBitmap(TopBGTexture0, 412, 240, GPU.Textures.ImageFormat.RGB565);
									break;
								case ThemeHeader.BGMoveType.FlipBook1:
								case ThemeHeader.BGMoveType.FlipBook2:
									BottomBGTexture = er.ReadBytes(1024 * 256 * 2);
									//b = GPU.Textures.ToBitmap(BottomBGTexture, 960, 240, GPU.Textures.ImageFormat.RGB565);
									break;
							}
							break;
						default:
							break;
					}
				}
				if (Header.UseSelector && Header.SelectorColorOffset != 0)
				{
					er.BaseStream.Position = Header.SelectorColorOffset;
					SelectorColor = new ThemeSelectorColor(er);
				}
				if (Header.UseFolderColor && Header.FolderColorOffset != 0)
				{
					er.BaseStream.Position = Header.FolderColorOffset;
					FolderColor = new ThemeFolderColor(er);
				}
				if (Header.UseFolderImage)
				{
					if (Header.FolderImageClosedOffset != 0)
					{
						er.BaseStream.Position = Header.FolderImageClosedOffset;
						FolderImageClosed = er.ReadBytes(128 * 64 * 3);
						//Bitmap b = GPU.Textures.ToBitmap(FolderImageClosed, 74, 64, GPU.Textures.ImageFormat.RGB8);
					}
					if (Header.FolderImageOpenOffset != 0)
					{
						er.BaseStream.Position = Header.FolderImageOpenOffset;
						FolderImageOpen = er.ReadBytes(128 * 64 * 3);
						//Bitmap b = GPU.Textures.ToBitmap(FolderImageOpen, 82, 64, GPU.Textures.ImageFormat.RGB8);
					}
				}
				if (Header.UseIconBGColor && Header.IconBGColorOffset != 0)
				{
					er.BaseStream.Position = Header.IconBGColorOffset;
					IconBGColor = new ThemeColorShadowAlpha(er);
				}
				if (Header.UseIconBGImage)
				{
					if (Header.IconBGImageLargeOffset != 0)
					{
						er.BaseStream.Position = Header.IconBGImageLargeOffset;
						IconBGImageLarge = er.ReadBytes(64 * 128 * 3);
						//Bitmap b = GPU.Textures.ToBitmap(IconBGImageLarge, 36, 72, GPU.Textures.ImageFormat.RGB8);
					}
					if (Header.IconBGImageSmallOffset != 0)
					{
						er.BaseStream.Position = Header.IconBGImageSmallOffset;
						IconBGImageSmall = er.ReadBytes(32 * 64 * 3);
						//Bitmap b = GPU.Textures.ToBitmap(IconBGImageSmall, 25, 50, GPU.Textures.ImageFormat.RGB8);
					}
				}
				if (Header.UseLRButtonColor && Header.LRButtonBGColorOffset != 0)
				{
					er.BaseStream.Position = Header.LRButtonBGColorOffset;
					LRButtonColor = new ThemeColorShadowAlpha(er);
				}
				if (Header.UseLRButtonArrowColor && Header.LRButtonArrowColorOffset != 0)
				{
					er.BaseStream.Position = Header.LRButtonArrowColorOffset;
					LRButtonArrowColor = new ThemeLRButtonArrowColor(er);
				}
				if (Header.UseBottomButtonColor)
				{
					if (Header.BottomButtonDefaultColorOffset != 0)
					{
						er.BaseStream.Position = Header.BottomButtonDefaultColorOffset;
						BottomButtonDefaultColor = new ThemeButtonColor(er);
					}
					if (Header.BottomButtonCloseColorOffset != 0)
					{
						er.BaseStream.Position = Header.BottomButtonCloseColorOffset;
						BottomButtonCloseColor = new ThemeButtonColor(er);
					}
				}
				if (Header.TitleBalloonType == ThemeHeader.BalloonType.Color && Header.TitleBalloonColorOffset != 0)
				{
					er.BaseStream.Position = Header.TitleBalloonColorOffset;
					TitleBalloonColor = new ThemeTextWithBGColor(er);
				}
				if (Header.UseIconPlateColor &&  Header.IconPlateColorOffset != 0)
				{
					er.BaseStream.Position = Header.IconPlateColorOffset;
					IconPlateColor = new ThemeColorShadowAlpha(er);
				}
				if (Header.UseIconPlateBGColor && Header.IconPlateBGColorOffset != 0)
				{
					er.BaseStream.Position = Header.IconPlateBGColorOffset;
					IconPlateBGColor = new ThemeIconPlateBGColor(er);
				}
				if (Header.UseFolderPlateColor &&  Header.FolderPlateColorOffset != 0)
				{
					er.BaseStream.Position = Header.FolderPlateColorOffset;
					FolderPlateColor = new ThemeColorShadowAlpha(er);
				}
				if (Header.UseFolderBackButtonColor && Header.FolderBackButtonColorOffset != 0)
				{
					er.BaseStream.Position = Header.FolderBackButtonColorOffset;
					FolderBackButtonColor = new ThemeButtonColor(er);
				}
				if (Header.UseSettingButtonColor && Header.SettingButtonColorOffset != 0)
				{
					er.BaseStream.Position = Header.SettingButtonColorOffset;
					SettingsButtonColor = new ThemeSettingButtonColor(er);
				}
				if (Header.UseCameraGuideTextColor && Header.CameraGuideTextColorOffset != 0)
				{
					er.BaseStream.Position = Header.CameraGuideTextColorOffset;
					CameraGuideTextColor = new ThemeTextWithBGColor(er);
				}
				if (Header.UseExtendedBannerTextColor && Header.ExtendedBannerTextColorOffset != 0)
				{
					er.BaseStream.Position = Header.ExtendedBannerTextColorOffset;
					ExtBannerTextColor = new ThemeExtBannerTextColor(er);
				}
				if (Header.UseSoundEffects && Header.SoundEffectSectionSize != 0 && Header.SoundEffectSectionOffset != 0)
				{
					//TODO!
				}
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new Form();
		}

		public ThemeHeader Header;
		public class ThemeHeader
		{
			public enum BGType : uint
			{
				None = 0,
				Color = 1,
				Pattern = 2,
				Wallpaper = 3
			}

			public enum BGMoveType : uint
			{
				Normal = 0,
				Fixed = 1,
				FlipBook1 = 2,
				Floating = 3,
				FlipBook2 = 4
			}

			public enum BalloonType : uint
			{
				None = 0,
				Color = 1,
				Invisible = 2
			}

			public ThemeHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				er.ReadPadding(0x10);
			}

			public UInt32 Version;
			public Byte RecommendMenuConfig;//Nr rows
			[BinaryBooleanSize(BooleanSize.U8)]
			public Boolean UseBackgroundMusic;
			[BinaryFixedSize(6)]
			public Byte[] Padding;

			public BGType TopBGType;
			public BGMoveType TopBGMoveType;
			public UInt32 TopBGColorOffset;
			public UInt32 TopBGTexture0Offset;
			public UInt32 TopBGTexture1Offset;

			public BGType BottomBGType;
			public BGMoveType BottomBGMoveType;
			public UInt32 BottomBGTextureOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseSelector;
			public UInt32 SelectorColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseFolderColor;
			public UInt32 FolderColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseFolderImage;
			public UInt32 FolderImageClosedOffset;
			public UInt32 FolderImageOpenOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseIconBGColor;
			public UInt32 IconBGColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseIconBGImage;
			public UInt32 IconBGImageLargeOffset;
			public UInt32 IconBGImageSmallOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseLRButtonColor;
			public UInt32 LRButtonBGColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseLRButtonArrowColor;
			public UInt32 LRButtonArrowColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseBottomButtonColor;
			public UInt32 BottomButtonDefaultColorOffset;
			public UInt32 BottomButtonCloseColorOffset;

			public BalloonType TitleBalloonType;
			public UInt32 TitleBalloonColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseIconPlateColor;
			public UInt32 IconPlateColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseIconPlateBGColor;
			public UInt32 IconPlateBGColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseFolderPlateColor;
			public UInt32 FolderPlateColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseFolderBackButtonColor;
			public UInt32 FolderBackButtonColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseSettingButtonColor;
			public UInt32 SettingButtonColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseCameraGuideTextColor;
			public UInt32 CameraGuideTextColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseExtendedBannerTextColor;
			public UInt32 ExtendedBannerTextColorOffset;

			[BinaryBooleanSize(BooleanSize.U32)]
			public Boolean UseSoundEffects;
			public UInt32 SoundEffectSectionSize;
			public UInt32 SoundEffectSectionOffset;
		}

		public ThemeTopBGColor TopBGColor;
		public Byte[] TopBGTexture0;
		public Byte[] TopBGTexture1;

		public Byte[] BottomBGTexture;

		public ThemeSelectorColor SelectorColor;

		public ThemeFolderColor FolderColor;

		public Byte[] FolderImageClosed;
		public Byte[] FolderImageOpen;

		public ThemeColorShadowAlpha IconBGColor;

		public Byte[] IconBGImageLarge;
		public Byte[] IconBGImageSmall;

		public ThemeColorShadowAlpha LRButtonColor;

		public ThemeLRButtonArrowColor LRButtonArrowColor;

		public ThemeButtonColor BottomButtonDefaultColor;
		public ThemeButtonColor BottomButtonCloseColor;

		public ThemeTextWithBGColor TitleBalloonColor;

		public ThemeColorShadowAlpha IconPlateColor;

		public ThemeIconPlateBGColor IconPlateBGColor;

		public ThemeColorShadowAlpha FolderPlateColor;

		public ThemeButtonColor FolderBackButtonColor;

		public ThemeSettingButtonColor SettingsButtonColor;

		public ThemeTextWithBGColor CameraGuideTextColor;

		public ThemeExtBannerTextColor ExtBannerTextColor;

		public class ThemeTopBGColor
		{
			public ThemeTopBGColor(EndianBinaryReaderEx er)
			{
				BGColor = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				GradientAlpha = er.ReadByte();
				PatternAlpha = er.ReadByte();
				Unknown1 = er.ReadByte();
				Unknown2 = er.ReadByte();
				er.ReadPadding(0x10);
			}

			public Color BGColor;
			public Byte GradientAlpha;
			public Byte PatternAlpha;
			public Byte Unknown1;
			public Byte Unknown2;
		}

		public class ThemeSelectorColor
		{
			public ThemeSelectorColor(EndianBinaryReaderEx er)
			{
				Dark = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Light = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Expand = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Color Dark;
			public Color Main;
			public Color Light;
			public Color Expand;
		}

		public class ThemeFolderColor
		{
			public ThemeFolderColor(EndianBinaryReaderEx er)
			{
				Dark = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Light = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Shadow = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Color Dark;
			public Color Main;
			public Color Light;
			public Color Shadow;
		}

		public class ThemeColorShadowAlpha
		{
			public ThemeColorShadowAlpha(EndianBinaryReaderEx er)
			{
				Dark = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Light = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Shadow = er.ReadColor8();
				er.ReadPadding(0x10);
			}
			public Color Dark;
			public Color Main;
			public Color Light;
			public Color Shadow;
		}

		public class ThemeLRButtonArrowColor
		{
			public ThemeLRButtonArrowColor(EndianBinaryReaderEx er)
			{
				Dark = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Expand = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Color Dark;
			public Color Main;
			public Color Expand;
		}

		public class ThemeButtonColor
		{
			public ThemeButtonColor(EndianBinaryReaderEx er)
			{
				TextShadowPosition = er.ReadSingle();
				Dark = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Light = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Shadow = er.ReadColor8();
				Expand = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				TextShadow = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				TextMain = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				TextSelect = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Single TextShadowPosition;
			public Color Dark;
			public Color Main;
			public Color Light;
			public Color Shadow;
			public Color Expand;
			public Color TextShadow;
			public Color TextMain;
			public Color TextSelect;
		}

		public class ThemeTextWithBGColor
		{
			public ThemeTextWithBGColor(EndianBinaryReaderEx er)
			{
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Light = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Shadow = er.ReadColor8();
				TextMain = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Color Main;
			public Color Light;
			public Color Shadow;
			public Color TextMain;
		}

		public class ThemeIconPlateBGColor
		{
			public ThemeIconPlateBGColor(EndianBinaryReaderEx er)
			{
				Dark = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				Light = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Color Dark;
			public Color Main;
			public Color Light;
		}

		public class ThemeSettingButtonColor
		{
			public ThemeSettingButtonColor(EndianBinaryReaderEx er)
			{
				BGDark = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				BGMain = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				BGLight = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				BGShadow = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				IconMain = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				IconLight = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				TextMain = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Color BGDark;
			public Color BGMain;
			public Color BGLight;
			public Color BGShadow;
			public Color IconMain;
			public Color IconLight;
			public Color TextMain;
		}

		public class ThemeExtBannerTextColor
		{
			public ThemeExtBannerTextColor(EndianBinaryReaderEx er)
			{
				Main = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				TextMain = Color.FromArgb(er.ReadByte(), er.ReadByte(), er.ReadByte());
				er.ReadPadding(0x10);
			}
			public Color Main;
			public Color TextMain;
		}

		public class ThemeIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "3DS Themes";
			}

			public override string GetFileDescription()
			{
				return "3DS Theme Data";
			}

			public override string GetFileFilter()
			{
				return "3DS Theme (body_LZ.bin)|body_LZ.bin";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Name.Equals("body_LZ.bin")) return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
