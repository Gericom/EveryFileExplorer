using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using _3DS.UI;

namespace _3DS
{
    public class ThemeFile : FileFormat<ThemeFile.ThemeIdentifier>, IViewable
    {
        public ThemeFile(byte[] Data)
        {
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                header = new ThemeHeader(er);

                header.bottomScreenFrameCount = (2 - header.bottomScreenFrameCount % 2) + header.bottomScreenFrameCount;
                header.topScreenFrameCount = (2 - header.topScreenFrameCount % 2) + header.topScreenFrameCount;
                System.Console.WriteLine("topScreenFrameCounttopScreenFrameCount " + header.topScreenFrameCount);

                bottomWidth = 256 * (int)header.bottomScreenFrameCount;
                topWidth = 512 * (int)header.topScreenFrameCount;
                bottomHeight = 240;
                topHeight = 240;

                er.BaseStream.Position = header.topScreenTextureOffset;
                topScreenTexture = er.ReadBytes((topHeight * topWidth) * 2);
                er.BaseStream.Position = header.bottomScreenTextureOffset;
                bottomScreenTexture = er.ReadBytes((bottomHeight * bottomWidth) * 2);



            }
            finally
            {
                er.Close();
            }
			
        }


        public int bottomWidth;
        public int bottomHeight;
        public byte[] bottomScreenTexture;

        public Bitmap GetBottomTexture()
        {
            return GPU.Textures.ToBitmap(bottomScreenTexture, bottomWidth, bottomHeight, GPU.Textures.ImageFormat.RGB565, true);
        }

        public int topWidth;
        public int topHeight;
        public byte[] topScreenTexture;

        public Bitmap GetTopTexture()
        {
            return GPU.Textures.ToBitmap(topScreenTexture, topWidth, topHeight, GPU.Textures.ImageFormat.RGB565, true);
        }

        public System.Windows.Forms.Form GetDialog()
        {
            return new ThemeViewer(this);
        }

        public ThemeHeader header;
        public class ThemeHeader
        {
            public ThemeHeader(EndianBinaryReader er)
			{
                version = er.ReadUInt32();
                er.ReadByte();
                er.ReadByte();
                er.ReadByte();
                er.ReadByte();
                er.ReadUInt32();
                er.ReadUInt32();
                topScreenFrameCount = er.ReadUInt32();
                topScreenSolidColorDataOffset = er.ReadUInt32();
                topScreenTextureOffset = er.ReadUInt32();
                er.ReadUInt32();
                bottomScreenFrameCount = er.ReadUInt32();
                bottomScreenSolidColorDataOffset = er.ReadUInt32();
                bottomScreenTextureOffset = er.ReadUInt32();
                er.ReadBytes(68);//unknown values
                useAudioSection = er.ReadUInt32() == 1;
                audioSectionSize = er.ReadUInt32();
                audioSectionOffset = er.ReadUInt32();
                System.Console.WriteLine("version " + version);
                System.Console.WriteLine("topScreenFrameCount " + topScreenFrameCount);
                System.Console.WriteLine("topScreenTextureOffset " + topScreenTextureOffset);
                System.Console.WriteLine("bottomScreenFrameCount " + bottomScreenFrameCount);
                System.Console.WriteLine("bottomScreenTextureOffset " + bottomScreenTextureOffset);


			}
			public UInt32 version;
            
            public UInt32 topScreenFrameCount;
            public UInt32 topScreenSolidColorDataOffset;
            public UInt32 topScreenTextureOffset;

            public UInt32 bottomScreenFrameCount;
            public UInt32 bottomScreenSolidColorDataOffset;
            public UInt32 bottomScreenTextureOffset;

            public Boolean useAudioSection;
            public UInt32 audioSectionSize;
            public UInt32 audioSectionOffset;
        }

        public class ThemeIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "3DS Themes";
            }

            public override string GetFileDescription()
            {
                return "System Theme body_LZ.bin";
            }

            public override string GetFileFilter()
            {
                return "System Menu Theme (body_LZ.bin)|body_LZ.bin";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Name.Equals("body_LZ.bin"))
                    return FormatMatch.Content;
                else
                    return FormatMatch.No;
            }

        }

    }



}
