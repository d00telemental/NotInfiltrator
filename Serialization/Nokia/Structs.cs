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

        /// <ghidra>im::m3g::Loader::LoadAnimationGroup</ghidra>
        public AnimationGroup(Stream stream)
        {
            var animationTargetCount = stream.ReadSigned16Little();
            AnimationTargets = new AnimationTarget[animationTargetCount];

            Debug.WriteLine($"Animation target count = {animationTargetCount}");

            for (int i = 0; i < animationTargetCount; i++)
            {
                AnimationTargets[i] = new()
                {
                    A = stream.ReadSigned32Little(),
                    B = stream.ReadSigned32Little(),
                    C = stream.ReadSigned32Little(),
                };

                Debug.WriteLine($"Animation target [{i}] = {{ A={AnimationTargets[i].A}, B={AnimationTargets[i].B}, C={AnimationTargets[i].C} }}");
            }

            var animationCount = stream.ReadSigned16Little();
            Animations = new Animation[animationCount];

            Debug.WriteLine($"Animation count = {animationCount}");

            for (int i = 0; i < animationCount; i++)
            {
                Animations[i] = new()
                {
                    A = stream.ReadAscFixed(stream.ReadSigned16Little()),
                    B = stream.ReadSigned32Little(),
                    C = stream.ReadSigned32Little(),
                    D = (byte)stream.ReadByte()
                };

                Debug.WriteLine($"Animation [{i}] = {{ A={Animations[i].A}, B={Animations[i].B}, C={Animations[i].C}, D={Animations[i].D} }}");
            }
        }
    }
}
