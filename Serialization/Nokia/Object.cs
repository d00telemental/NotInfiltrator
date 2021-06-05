﻿using System;
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
                //throw new Exception("UNREAD DATA");
            }
        }

        public static Object Read(AgnosticObjectInfo info)
        {
            if (!Enum.IsDefined(typeof(ObjectType), info.Type))
            {
                throw new Exception($"Unrecognized object type {info.Type}");
            }

            return (ObjectType)info.Type switch
            {
                ObjectType.Header => new HeaderObject(info.Data),              // 00
                ObjectType.AnimationController => new AnimationController(info.Data),  // 01
                ObjectType.AnimationTrack => new AnimationTrack(info.Data),    // 02
                ObjectType.Appearance => new Appearance(info.Data),            // 03
                ObjectType.CompositingMode => new CompositingMode(info.Data),  // 06
                ObjectType.PolygonMode => new PolygonMode(info.Data),          // 08
                ObjectType.Group => new Group(info.Data, true),                // 09
                ObjectType.Image2D => new Image2D(info.Data),                  // 10
                ObjectType.Mesh => new Mesh(info.Data),                        // 14 | something's up with the submesh looped part
                ObjectType.Texture2D => new Texture2D(info.Data),              // 17
                ObjectType.VertexArray => new VertexArray(info.Data),          // 20
                ObjectType.VertexBuffer => new VertexBuffer(info.Data),        // 21 | unknown 2 object indices in the end

                ObjectType.Unknown100 => new AGenericObject(info.Data),  // NOT PARSED | 100 = SUBMESH, 144 = TEXTURECUBE
                ObjectType.Unknown101 => new AGenericObject(info.Data),  // NOT PARSED

                _ => throw new Exception($"Unsupported object type {info.Type}")
            };
        }
    }

    #region Non-abstract M3G classes inheriting from Object
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
    #endregion


    public abstract class Object3D : Object
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

    #region Abstract M3G classes inheriting from Object3D
    public abstract class AppearanceBase : Object3D
    {
        public static new ObjectType? Type => null;

        public byte Layer { get; set; }
        public UInt32 CompositingMode { get; set; }

        public AppearanceBase(byte[] sourceData)
            : base(sourceData)
        {
            Layer = (byte)DataStream.ReadByte();
            CompositingMode = DataStream.ReadUnsigned32Little();
        }
    }

    public abstract class Transformable : Object3D
    {
        public static new ObjectType? Type => null;

        public bool HasComponentTransform { get; set; }
        public Vector3D? Translation { get; set; } = null;
        public Vector3D? Scale { get; set; } = null;
        public float? OrientationAngle { get; set; } = null;
        public Vector3D? OrientationAxis { get; set; } = null;

        public bool HasGeneralTransform { get; set; }
        public Matrix? Transform { get; set; } = null;

        public Transformable(byte[] sourceData)
            : base(sourceData)
        {
            HasComponentTransform = DataStream.ReadBool();
            if (HasComponentTransform)
            {
                Translation = new Vector3D(DataStream);
                Scale = new Vector3D(DataStream);
                OrientationAngle = DataStream.ReadSingleSlow();
                OrientationAxis = new Vector3D(DataStream);
            }
            HasGeneralTransform = DataStream.ReadBool();
            if (HasGeneralTransform)
            {
                Transform = new Matrix(DataStream);
            }
        }
    }

    public abstract class Node : Transformable
    {
        public static new ObjectType? Type => null;

        public bool EnableRendering { get; set; }
        public bool EnablePicking { get; set; }
        public byte AlphaFactor { get; set; }
        public Int32 Scope { get; set; }
        public bool HasAlignment { get; set; }
        public AnimationGroup? AnimGroupQM { get; set; }

        public Node(byte[] sourceData)
            : base(sourceData)
        {
            // wtf in AnimationGroup?!
            EnableRendering = DataStream.ReadBool();
            EnablePicking = DataStream.ReadBool();
            AlphaFactor = (byte)DataStream.ReadByte();
            Scope = DataStream.ReadSigned32Little();
            HasAlignment = DataStream.ReadBool();

            if (HasAlignment)
            {
                Debug.WriteLine($"Read'' HasAlignment, Pos={DataStream.Position} Len={DataStream.Length}");
                //Debug.WriteLine($"{BitConverter.ToString(DataStream.ReadBytes((int)(DataStream.Length - DataStream.Position))).Replace('-', ' ')}");
                AnimGroupQM = new AnimationGroup(DataStream);
            }
        }
    }

    public abstract class Texture : Transformable
    {
        public static new ObjectType? Type => null;

        public UInt32 Image { get; set; }

        public Texture(byte[] sourceData)
            : base(sourceData)
        {
            Image = DataStream.ReadUnsigned32Little();
        }
    }
    #endregion

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

    #region Non-abstract M3G classes inheriting from Object3D
    public class AnimationController : Object3D
    {
        public static new ObjectType? Type => ObjectType.AnimationController;

        public float Speed { get; set; }
        public float Weight { get; set; }
        public Int32 ActiveIntervalStart { get; set; }
        public Int32 ActiveIntervalEnd { get; set; }
        public float ReferenceSequenceTime { get; set; }
        public Int32 ReferenceWorldTime { get; set; }

        public AnimationController(byte[] sourceData)
            : base(sourceData)
        {
            Speed = DataStream.ReadSingleSlow();
            Weight = DataStream.ReadSingleSlow();
            ActiveIntervalStart = DataStream.ReadSigned32Little();
            ActiveIntervalEnd = DataStream.ReadSigned32Little();
            ReferenceSequenceTime = DataStream.ReadSingleSlow();
            ReferenceWorldTime = DataStream.ReadSigned32Little();

            AssertEndStream();
        }
    }

    public class AnimationTrack : Object3D
    {
        public static new ObjectType? Type => ObjectType.AnimationTrack;

        public UInt32 KeyframeSequence { get; set; }
        public UInt32 AnimationController { get; set; }
        public Int32 PropertyID { get; set; }

        public AnimationTrack(byte[] sourceData)
            : base(sourceData)
        {
            KeyframeSequence = DataStream.ReadUnsigned32Little();
            AnimationController = DataStream.ReadUnsigned32Little();
            PropertyID = DataStream.ReadSigned32Little();

            AssertEndStream();
        }
    }

    public class Appearance : AppearanceBase
    {
        public static new ObjectType? Type => ObjectType.Appearance;

        public UInt32 Fog { get; set; }
        public UInt32 PolygonMode { get; set; }
        public UInt32 Material { get; set; }
        public UInt32[] Textures { get; set; }

        public Appearance(byte[] data)
            : base(data)
        {
            Fog = DataStream.ReadUnsigned32Little();
            PolygonMode = DataStream.ReadUnsigned32Little();
            Material = DataStream.ReadUnsigned32Little();

            var textureCount = DataStream.ReadUnsigned32Little();
            Textures = new UInt32[textureCount];
            for (uint i = 0; i < textureCount; i++)
            {
                Textures[i] = DataStream.ReadUnsigned32Little();
            }

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
        public int Blending { get; set; } // DEVIATES FROM SPEC STARTING HERE
        public float AlphaThreshold { get; set; }  
        public float DepthOffsetFactor { get; set; }
        public float DepthOffsetUnits { get; set; }

        public CompositingMode(byte[] sourceData)
            : base(sourceData)
        {
            DepthTestEnabled = DataStream.ReadBool();
            DepthWriteEnabled = DataStream.ReadBool();
            ColorWriteEnabled = DataStream.ReadBool();
            AlphaWriteEnabled = DataStream.ReadBool();

            Blending = DataStream.ReadSigned32Little();
            AlphaThreshold = DataStream.ReadSigned32Little();
            DepthOffsetFactor = DataStream.ReadSigned32Little();
            DepthOffsetUnits = DataStream.ReadSigned32Little();

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

    public class Group : Node
    {
        public static new ObjectType? Type => ObjectType.Group;

        public UInt32[] Children { get; set; }

        public Group(byte[] sourceData, bool finalChild)
            : base(sourceData)
        {
            var childCount = DataStream.ReadUnsigned32Little();
            Children = new uint[childCount];
            for (uint i = 0; i < childCount; i++)
            {
                Children[i] = DataStream.ReadUnsigned32Little();
            }
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

    public class Mesh : Node
    {
        public static new ObjectType? Type => ObjectType.Mesh;

        public UInt32 VertexBuffer { get; set; }
        public UInt32 SubmeshCount { get; set; }

        public Mesh(byte[] sourceData)
            : base(sourceData)
        {
            VertexBuffer = DataStream.ReadUnsigned32Little();
            SubmeshCount = DataStream.ReadUnsigned32Little();
            if (SubmeshCount != 1)
            {
                throw new Exception("DEBUG ME DEBUG ME PLSSS");
            }
            for (uint i = 0; i < SubmeshCount; i++)
            {
                _ = DataStream.ReadUnsigned32Little(); // this should be indexBuffer, but appearance should be next and its missing
            }
            AssertEndStream();
        }
    }

    public class Texture2D : Texture
    {
        public static new ObjectType? Type => ObjectType.Texture2D;

        public RGB BlendColor { get; set; }
        public byte Blending { get; set; }
        public byte WrappingS { get; set; }
        public byte WrappingT { get; set; }
        public byte LevelFilter { get; set; }
        public byte ImageFilter { get; set; }

        public Texture2D(byte[] sourceData)
            : base(sourceData)
        {
            BlendColor = new RGB(DataStream);
            Blending = (byte)DataStream.ReadByte();
            WrappingS = (byte)DataStream.ReadByte();
            WrappingT = (byte)DataStream.ReadByte();
            LevelFilter = (byte)DataStream.ReadByte();
            ImageFilter = (byte)DataStream.ReadByte();
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

        public RGBA ColorRGBA { get; set; }
        public UInt32 Positions { get; set; }
        public Single[] PositionBias { get; set; } = new Single[3];
        public Single PositionScale { get; set; }
        public UInt32 Normals { get; set; }
        public UInt32 Colors { get; set; }
        public Int32 TexcoordArrayCount { get; set; }  // in deviation from the spec, should always be -1 and not UInt32   #logic

        public class TexCoordInfo
        {
            public UInt32 TexCoords { get; set; }
            public Single[] TexCoordsBias { get; set; } = new Single[3];
            public Single TexCoordsScale { get; set; }
        }
        public List<TexCoordInfo> TexCoords { get; set; } = new ();

        public UInt32 SomeObjectIndex1 { get; set; }
        public UInt32 SomeObjectIndex2 { get; set; }

        public VertexBuffer(byte[] sourceData)
            : base(sourceData)
        {
            ColorRGBA = new RGBA(DataStream);
            Positions = DataStream.ReadUnsigned32Little();
            for (int i = 0; i < 3; i++) PositionBias[i] = DataStream.ReadSingleSlow();
            PositionScale = DataStream.ReadSingleSlow();
            Normals = DataStream.ReadUnsigned32Little();
            Colors = DataStream.ReadUnsigned32Little();
            TexcoordArrayCount = DataStream.ReadSigned32Little();
            Common.Assert(TexcoordArrayCount == -1, "TexcoordArrayCount was not -1");
            for (int j = 0; j < 1 /* YES I'M NOT KIDDING. YES IT'S HORRIBLE. YOU KNOW WHAT'S MORE HORRIBLE? THIS FUCKING ENGINE */; j++)
            {
                TexCoordInfo tci = new ();
                tci.TexCoords = DataStream.ReadUnsigned32Little();
                for (int i = 0; i < 3; i++) tci.TexCoordsBias[i] = DataStream.ReadSingleSlow();
                tci.TexCoordsScale = DataStream.ReadSingleSlow();
                TexCoords.Add(tci);
            }
            SomeObjectIndex1 = DataStream.ReadUnsigned32Little();
            SomeObjectIndex2 = DataStream.ReadUnsigned32Little();
            AssertEndStream();
        }
    }
    #endregion
}
