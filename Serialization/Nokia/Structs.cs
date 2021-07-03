using NotInfiltrator.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    namespace Vectormath
    {
        [DebuggerDisplay("({A};{B};{C};{D})")]
        public struct Vector4
        {
            public float A { get; set; }
            public float B { get; set; }
            public float C { get; set; }
            public float D { get; set; }

            public Vector4(Stream stream)
            {
                A = stream.ReadSingleSlow();
                B = stream.ReadSingleSlow();
                C = stream.ReadSingleSlow();
                D = stream.ReadSingleSlow();
            }

            public override string ToString()
                => $"{{{A};{B};{C};{D}}}";
        }

        [DebuggerDisplay("( {A};{B};{C};{D} )")]
        public struct Matrix4
        {
            public Vector4 A { get; set; }
            public Vector4 B { get; set; }
            public Vector4 C { get; set; }
            public Vector4 D { get; set; }

            public Matrix4(Stream stream)
            {
                A = new Vector4(stream);
                B = new Vector4(stream);
                C = new Vector4(stream);
                D = new Vector4(stream);
            }

            public override string ToString()
                => $"{{ {A};{B};{C};{D} }}";
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public struct KeyframeData
    {
        public uint Time { get; set; }
        public List<float> VectorValue { get; set; }
        public override string ToString()
            => $"Time = {Time}; VectorValue = {String.Join(", ", VectorValue)}";
    }

    public struct AnimationGroup
    {
        /// <summary>
        /// TODO: give proper names
        /// </summary>
        public struct AnimationTarget
        {
            public Int32 A { get; set; }
            public Int32 B { get; set; }
            public Int32 C { get; set; }
        }

        /// <summary>
        /// TODO: give proper names
        /// </summary>
        public struct Animation
        {
            public string A { get; set; }
            public Int32 B { get; set; }
            public Int32 C { get; set; }
            public byte D { get; set; }
        }

        public AnimationTarget[] AnimationTargets { get; set; }
        public Animation[] Animations { get; set; }

        public AnimationGroup(Stream stream)
        {
            var animationTargetCount = stream.ReadSigned16Little();
            AnimationTargets = new AnimationTarget[animationTargetCount];

            for (int i = 0; i < animationTargetCount; i++)
            {
                AnimationTargets[i] = new()
                {
                    A = stream.ReadSigned32Little(),
                    B = stream.ReadSigned32Little(),
                    C = stream.ReadSigned32Little(),
                };
            }

            var animationCount = stream.ReadSigned16Little();
            Animations = new Animation[animationCount];

            for (int i = 0; i < animationCount; i++)
            {
                Animations[i] = new()
                {
                    A = stream.ReadAscFixed(stream.ReadSigned16Little()),
                    B = stream.ReadSigned32Little(),
                    C = stream.ReadSigned32Little(),
                    D = (byte)stream.ReadByte()
                };
            }
        }
    }
}
