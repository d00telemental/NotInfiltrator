using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Nokia
{
    public struct RGBA
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte Alpha { get; set; }

        public RGBA(Stream stream)
        {
            Red = (byte)stream.ReadByte();
            Green = (byte)stream.ReadByte();
            Blue = (byte)stream.ReadByte();
            Alpha = (byte)stream.ReadByte();
        }
    }
}
