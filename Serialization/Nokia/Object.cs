using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Nokia
{
    public abstract class Object
    {
        public virtual ObjectType? Type { get; protected set; } = null;
        public byte[] RawData { get; protected set; } = null;

        protected Stream DataStream { get; set; }

        public Object(byte[] sourceData)
        {
            RawData = sourceData;
            DataStream = new MemoryStream(RawData);
        }

        protected void AssertEndStream()
        {
            if (DataStream.Position != DataStream.Length)
            {
                Debug.WriteLine($"{BitConverter.ToString(DataStream.ReadBytes((int)(DataStream.Length - DataStream.Position))).Replace('-', ' ')}");
                throw new Exception("UNREAD DATA");
            }
        }

        public static Object Read(AgnosticObjectInfo info)
        {
            return info.Type switch
            {
                (byte)ObjectType.Header => new HeaderObject(info.Data),
                (byte)ObjectType.CompositingMode => new CompositingMode(info.Data),  // unknown 6 bytes in the end
                (byte)ObjectType.PolygonMode => new PolygonMode(info.Data),
                (byte)ObjectType.Image2D => new Image2D(info.Data),
                (byte)ObjectType.VertexArray => new VertexArray(info.Data),
                (byte)ObjectType.VertexBuffer => new VertexBuffer(info.Data),

                (byte)ObjectType.Unknown100 => new AGenericObject(info.Data),
                (byte)ObjectType.Unknown101 => new AGenericObject(info.Data),  // a really big chungus

                _ => throw new Exception($"Unrecognized object type {info.Type}")
            };
        }
    }

    public class HeaderObject : Object
    {
        public static new ObjectType? Type => ObjectType.Header;

        public byte VersionNumberMajor { get; set; }
        public byte VersionNumberMinor { get; set; }
        public bool HasExternalReferences { get; set; }
        public UInt32 TotalFileSize { get; set; }
        public UInt32 ApproximateContentSize { get; set; }
        public string AuthoringField { get; set; }

        public HeaderObject(byte[] sourceData)
            : base(sourceData)
        {
            VersionNumberMajor = (byte)DataStream.ReadByte();
            VersionNumberMinor = (byte)DataStream.ReadByte();
            HasExternalReferences = DataStream.ReadByte() == 0 ? false : true;
            TotalFileSize = DataStream.ReadUnsigned32Little();
            ApproximateContentSize = DataStream.ReadUnsigned32Little();
            AuthoringField = DataStream.ReadAscTerminated(1500);

            AssertEndStream();
        }
    }

    public class Object3D : Object
    {
        public struct UserParameter
        {
            public UInt32 ID { get; set; }
            public Byte[] Value { get; set; }
        }

        public static new ObjectType? Type => null;

        public UInt32 UserID { get; set; }
        public List<UInt32> AnimationTracks { get; set; } = new List<uint>();
        public List<UserParameter> UserParameters { get; set; } = new List<UserParameter>();

        public Object3D(byte[] sourceData)
            : base(sourceData)
        {
            UserID = DataStream.ReadUnsigned32Little();

            var animationTrackCount = DataStream.ReadUnsigned32Little();
            for (int i = 0; i < animationTrackCount; i++)
            {
                var animationTrack = DataStream.ReadUnsigned32Little();
                AnimationTracks.Add(animationTrack);
            }

            var userParamCount = DataStream.ReadUnsigned32Little();
            for (int i = 0; i < userParamCount; i++)
            {
                UserParameter userParameter = new ();
                userParameter.ID = DataStream.ReadUnsigned32Little();
                userParameter.Value = DataStream.ReadBytes((int)DataStream.ReadUnsigned32Little());
                UserParameters.Add(userParameter);
            }
        }
    }

    public class AGenericObject : Object3D
    {
        public static new ObjectType? Type => null;

        public byte[] UnknownData { get; set; } = null;

        public AGenericObject(byte[] sourceData)
            : base(sourceData)
        {
            UnknownData = DataStream.ReadBytes((int)(DataStream.Length - DataStream.Position));
            AssertEndStream();
        }
    }

    public class CompositingMode : Object3D
    {
        public static new ObjectType? Type => ObjectType.CompositingMode;

        public bool DepthTestEnabled { get; set; }
        public bool DepthWriteEnabled { get; set; }
        public bool ColorWriteEnabled { get; set; }
        public bool AlphaWriteEnabled { get; set; }

        public byte Blending { get; set; }
        public byte AlphaThreshold { get; set; }
        public float DepthOffsetFactor { get; set; }
        public float DepthOffsetUnits { get; set; }

        public byte[] UnknownSixBytes { get; set; }

        public CompositingMode(byte[] sourceData)
            : base(sourceData)
        {
            DepthTestEnabled = DataStream.ReadBool();
            DepthWriteEnabled = DataStream.ReadBool();
            ColorWriteEnabled = DataStream.ReadBool();
            AlphaWriteEnabled = DataStream.ReadBool();

            Blending = (byte)DataStream.ReadByte();
            AlphaThreshold = (byte)DataStream.ReadByte();
            DepthOffsetFactor = new BinaryReader(new MemoryStream(DataStream.ReadBytes(4))).ReadSingle();
            DepthOffsetUnits = new BinaryReader(new MemoryStream(DataStream.ReadBytes(4))).ReadSingle();

            UnknownSixBytes = DataStream.ReadBytes(6);

            AssertEndStream();
        }
    }

    public class PolygonMode : Object3D
    {
        public static new ObjectType? Type => ObjectType.PolygonMode;

        public byte Culling { get; set; }
        public byte Shading { get; set; }
        public byte Winding { get; set; }
        public bool TwoSidedLightingEnabled { get; set; }
        public bool LocalCameraLightingEnabled { get; set; }
        public bool PerspectiveCorrectionEnabled { get; set; }

        public PolygonMode(byte[] sourceData)
            : base(sourceData)
        {
            Culling = (byte)DataStream.ReadByte();
            Shading = (byte)DataStream.ReadByte();
            Winding = (byte)DataStream.ReadByte();

            TwoSidedLightingEnabled = DataStream.ReadBool();
            LocalCameraLightingEnabled = DataStream.ReadBool();
            PerspectiveCorrectionEnabled = DataStream.ReadBool();

            AssertEndStream();
        }
    }

    public class Image2D : Object3D
    {
        public static new ObjectType? Type => ObjectType.Image2D;

        public byte Format { get; set; }
        public bool IsMutable { get; set; }
        public UInt32 Width { get; set; }
        public UInt32 Height { get; set; }

        public byte[] PaletteBytes { get; set; }
        public byte[] PixelsBytes { get; set; }

        public Image2D(byte[] sourceData)
            : base(sourceData)
        {
            Format = (byte)DataStream.ReadByte();
            IsMutable = DataStream.ReadBool();
            Width = DataStream.ReadUnsigned32Little();
            Height = DataStream.ReadUnsigned32Little();

            if (!IsMutable)
            {
                PaletteBytes = DataStream.ReadBytes((int)DataStream.ReadUnsigned32Little());
                PixelsBytes = DataStream.ReadBytes((int)DataStream.ReadUnsigned32Little());
            }

            AssertEndStream();
        }
    }

    public class VertexArray : Object3D
    {
        public static new ObjectType? Type => ObjectType.VertexArray;

        public byte ComponentSize { get; set; }
        public byte ComponentCount { get; set; }
        public byte Encoding { get; set; }

        public List<Int16[]> Vertices { get; set; } = new ();

        public VertexArray(byte[] sourceData)
            : base(sourceData)
        {
            ComponentSize = (byte)DataStream.ReadByte();
            ComponentCount = (byte)DataStream.ReadByte();
            Encoding = (byte)DataStream.ReadByte();

            Common.Assert(ComponentCount >= 2 && ComponentCount <= 4, $"Unsupported componentCount {ComponentCount}!");
            Common.Assert(ComponentSize == 1 || ComponentSize == 2, $"Unsupported componentSize {ComponentSize}!");
            Common.Assert(Encoding == 0 || Encoding == 1, $"Unsupported encoding {Encoding}!");

            var vertexCount = DataStream.ReadUnsigned16Little();

            if (ComponentSize == 1)  // byte
            {
                if (Encoding == 0)  // absolute components
                {
                    for (int vertexIdx = 0; vertexIdx < vertexCount; vertexIdx++)
                    {
                        var components = new byte[ComponentCount];
                        for (int i = 0; i < ComponentCount; i++)
                        {
                            components[i] = (byte)DataStream.ReadByte();
                        }
                        Vertices.Add(components.Select(b => (short)b).ToArray());
                    }
                }
                else  // delta components
                {
                    var accumulator = new byte[ComponentCount];
                    for (int accumIdx = 0; accumIdx < ComponentCount; accumIdx++)
                    {
                        accumulator[accumIdx] = 0;
                    }

                    for (int vertexIdx = 0; vertexIdx < vertexCount; vertexIdx++)
                    {
                        for (int i = 0; i < ComponentCount; i++)
                        {
                            accumulator[i] += (byte)DataStream.ReadByte();
                        }

                        var components = new byte[ComponentCount];
                        Array.Copy(accumulator, 0, components, 0, ComponentCount);
                        Vertices.Add(components.Select(b => (short)b).ToArray());
                    }
                }
            }
            else if (ComponentSize == 2)  // short
            {
                if (Encoding == 0)  // absolute components
                {
                    for (int vertexIdx = 0; vertexIdx < vertexCount; vertexIdx++)
                    {
                        var components = new short[ComponentCount];
                        for (int i = 0; i < ComponentCount; i++)
                        {
                            components[i] = DataStream.ReadSigned16Little();
                        }
                        Vertices.Add(components);
                    }
                }
                else  // delta components
                {
                    var accumulator = new short[ComponentCount];
                    for (int accumIdx = 0; accumIdx < ComponentCount; accumIdx++)
                    {
                        accumulator[accumIdx] = 0;
                    }

                    for (int vertexIdx = 0; vertexIdx < vertexCount; vertexIdx++)
                    {
                        for (int i = 0; i < ComponentCount; i++)
                        {
                            accumulator[i] += DataStream.ReadSigned16Little();
                        }

                        var components = new short[ComponentCount];
                        Array.Copy(accumulator, 0, components, 0, ComponentCount);
                        Vertices.Add(components);
                    }
                }
            }

            AssertEndStream();
        }
    }

    public class VertexBuffer : Object3D
    {
        public static new ObjectType? Type => ObjectType.VertexBuffer;

        

        public VertexBuffer(byte[] sourceData)
            : base(sourceData)
        {
            AssertEndStream();
        }
    }
}
