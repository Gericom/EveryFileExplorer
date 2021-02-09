using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Collections;

namespace GCNWii
{
    class BCO
    {
        public static void Read(string infile, string outfile)
        {
            EndianBinaryReader er = new EndianBinaryReader(File.OpenRead(infile), Endianness.BigEndian);
            er.BaseStream.Position = 0x24;
            uint offset = er.ReadUInt32();
            uint end = er.ReadUInt32();
            er.BaseStream.Position = offset;
            List<Vector3> vert = new List<Vector3>();
            List<ushort> types = new List<ushort>();
            while (er.BaseStream.Position != end)
            {
                vert.Add(new Vector3(er.ReadSingle(), er.ReadSingle(), er.ReadSingle()));
            }
            er.BaseStream.Position = 0x20;
            offset = er.ReadUInt32();
            end = er.ReadUInt32();
            er.BaseStream.Position = offset;
            List<Vector3> plane = new List<Vector3>();
            while (er.BaseStream.Position != end)
            {
                plane.Add(new Vector3(er.ReadUInt32(), er.ReadUInt32(), er.ReadUInt32()));
                er.ReadBytes(10);
                ushort type = er.ReadUInt16();
                types.Add(type);
                er.ReadBytes(12);
            }
            er.Close();
            File.Create(outfile).Close();
            TextWriter tw = new StreamWriter(outfile);
            tw.WriteLine("# Created by EveryFileExplorer");
            try
            {
                for (int i = 0; i < vert.Count; i++)
                {
                    tw.WriteLine("v {0} {1} {2}", (vert[i].X).ToString().Replace(",", "."), (vert[i].Y).ToString().Replace(",", "."), (vert[i].Z).ToString().Replace(",", "."));
                    //tw.WriteLine("v {0} {1} {2}", (vert[(int)plane[i].Y].X).ToString().Replace(",", "."), (vert[(int)plane[i].Y].Y).ToString().Replace(",", "."), (vert[(int)plane[i].Y].Z).ToString().Replace(",", "."));
                    //tw.WriteLine("v {0} {1} {2}", (vert[(int)plane[i].Z].X).ToString().Replace(",", "."), (vert[(int)plane[i].Z].Y).ToString().Replace(",", "."), (vert[(int)plane[i].Z].Z).ToString().Replace(",", "."));
                }
                for (int i = 0; i < plane.Count; i++)
                {
                    tw.WriteLine("usemtl " + types[i].ToString());
                    tw.WriteLine("f {0} {1} {2}", plane[i].X + 1, plane[i].Y + 1, plane[i].Z + 1);
                }
                tw.Close();
            }
            catch
            {
                tw.Close();
            }
        }
    }
}
