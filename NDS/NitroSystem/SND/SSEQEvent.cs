using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Midi;

namespace NDS.NitroSystem.SND
{
    public class SSEQEvent
    {
        public int Type;
        public byte[] Params;
        public SSEQEvent(int Type, byte[] Params)
        {
            this.Type = Type;
            this.Params = Params;
        }
    }
}