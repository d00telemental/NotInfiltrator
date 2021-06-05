using NotInfiltrator.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Nokia
{
    public struct RGB
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public RGB(Stream stream)
        {
            Red = (byte)stream.ReadByte();
            Green = (byte)stream.ReadByte();
            Blue = (byte)stream.ReadByte();
        }
    }

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

    public struct Vector3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3D(Stream stream)
        {
            X = stream.ReadSingleSlow();
            Y = stream.ReadSingleSlow();
            Z = stream.ReadSingleSlow();
        }
    }

    public struct Matrix
    {
        public float[] Elements { get; set; }


        public Matrix(Stream stream)
        {
            Elements = new float[16];
            for (int i = 0; i < 16; i++)
            {
                Elements[i] = stream.ReadSingleSlow();  // TODO: this should be optimized the fuck away
            }
        }
    }
}
