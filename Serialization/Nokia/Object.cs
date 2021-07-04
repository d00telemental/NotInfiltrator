using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
                DataStream.DebugRemainingBytes();
                Debugger.Break();
                throw new Exception("AssertEndStream: unread data!");
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
                ObjectType.Mesh => new Mesh(info.Data, true),                  // 14
                ObjectType.SkinnedMesh => new SkinnedMesh(info.Data),          // 16
                ObjectType.Texture2D => new Texture2D(info.Data),              // 17
                ObjectType.KeyframeSequence => new KeyframeSequence(info.Data),  // 19
                ObjectType.VertexArray => new VertexArray(info.Data),          // 20
                ObjectType.VertexBuffer => new VertexBuffer(info.Data),        // 21

                // Submeshes:

                ObjectType.Unknown100 => new Unknown100(info.Data),            // 100
                ObjectType.Unknown102 => new Unknown102(info.Data),            // 102
                ObjectType.Unknown103 => new Unknown103(info.Data),            // 103

                // ?

                ObjectType.Unknown101 => null,                                 // 101

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
                //var seekOffset = (int)(DataStream.Length - DataStream.Position);
                //Debug.WriteLine($"Read'' HasAlignment, Pos={DataStream.Position} Len={DataStream.Length}");
                //Debug.WriteLine($"{BitConverter.ToString(DataStream.ReadBytes(seekOffset)).Replace('-', ' ')}");
                //DataStream.Seek(-seekOffset, SeekOrigin.Current);

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
        public UInt32[] SubmeshReferences { get; set; }

        public Mesh(byte[] sourceData, bool finalChild)
            : base(sourceData)
        {
            VertexBuffer = DataStream.ReadUnsigned32Little();

            var submeshCount = DataStream.ReadUnsigned32Little();
            SubmeshReferences = new UInt32[submeshCount];
            for (uint i = 0; i < submeshCount; i++)
            {
                SubmeshReferences[i] = DataStream.ReadUnsigned32Little();
            }

            if (finalChild)
            {
                AssertEndStream();
            }
        }
    }

    public class SkinnedMesh : Mesh
    {
        [DebuggerDisplay("Ref = {Ref}, Matrix = {Matrix}")]
        public struct BonePaletteItem
        {
            public UInt32 Ref { get; set; }
            public Vectormath.Matrix4? Matrix { get; set; }

            public override string ToString()
                => $"Ref = {Ref}, Matrix = {Matrix}";
        }

        public static new ObjectType? Type => ObjectType.SkinnedMesh;

        public UInt32 SkeletonGroup { get; set; }
        public UInt32 AVertexArrayFieldB { get; set; }
        public UInt32 AVertexArrayFieldC { get; set; }

        public Vectormath.Matrix4? WeirdRareMatrix { get; set; } = null;

        public int BonePaletteSize { get; set; }
        public List<BonePaletteItem> BonePaletteItems { get; set; }

        public SkinnedMesh(byte[] sourceData)
            : base(sourceData, false)
        {
            SkeletonGroup = DataStream.ReadUnsigned32Little();
            AVertexArrayFieldB = DataStream.ReadUnsigned32Little();
            AVertexArrayFieldC = DataStream.ReadUnsigned32Little();

            var bonePaletteSize = DataStream.ReadSigned32Little();
            bool wasNegative = false;

            if (bonePaletteSize < 0)  // literally happens in just 1 file
            {
                BonePaletteSize = -bonePaletteSize;
                wasNegative = true;

                Vectormath.Matrix4 matrix4 = new(DataStream);
                WeirdRareMatrix = matrix4;
            }
            else
            {
                BonePaletteSize = bonePaletteSize;
                // ???
            }

            BonePaletteItems = new();

            for (int i = 0; i < BonePaletteSize; i++)
            {
                UInt32 nodeReference = DataStream.ReadUnsigned32Little();
                if (!wasNegative)
                {
                    BonePaletteItems.Add(new() { Ref = nodeReference, Matrix = null });
                }
                else
                {
                    Vectormath.Matrix4 matrix4 = new(DataStream);
                    BonePaletteItems.Add(new() { Ref = nodeReference, Matrix = matrix4 });
                }
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

    public class KeyframeSequence : Object3D
    {
        public static new ObjectType? Type => ObjectType.KeyframeSequence;

        public byte Interpolation { get; set; }
        public byte RepeatMode { get; set; }
        public byte Encoding { get; set; }  // 0 (float) and 2 (short) are supported, 1 (byte) is not

        public uint Duration { get; set; }
        public uint ValidRangeFirst { get; set; }
        public uint ValidRangeLast { get; set; }

        public uint ComponentCount { get; set; }
        public uint KeyframeCount { get; set; }

        public List<float> VectorBias { get; set; } = null;
        public List<float> VectorScale { get; set; } = null;

        public List<KeyframeData> Keyframes { get; set; } = new();

        public KeyframeSequence(byte[] sourceData)
            : base(sourceData)
        {
            Interpolation = (byte)DataStream.ReadByte();
            RepeatMode = (byte)DataStream.ReadByte();
            Encoding = (byte)DataStream.ReadByte();

            Duration = DataStream.ReadUnsigned32Little();
            ValidRangeFirst = DataStream.ReadUnsigned32Little();
            ValidRangeLast = DataStream.ReadUnsigned32Little();

            ComponentCount = DataStream.ReadUnsigned32Little();
            KeyframeCount = DataStream.ReadUnsigned32Little();

            var mappableLayout = (Encoding & 0x80) != 0;
            var realEncodingByte = Encoding & 0x7f;  // cut off at 7 bits

            if (!mappableLayout /*|| realEncodingByte != 2u*/) Debugger.Break();

            switch (realEncodingByte)
            {
                case 0:
                    {
                        // Unencoded floats.

                        if (mappableLayout)
                        {
                            var keyframes = new KeyframeData[KeyframeCount];

                            // Allocate keyframe objects.
                            for (int i = 0; i < KeyframeCount; i++)
                            {
                                keyframes[i] = new();
                            }

                            // Read times.
                            for (int i = 0; i < KeyframeCount; i++)
                            {
                                keyframes[i].Time = DataStream.ReadUnsigned32Little();
                            }

                            // Read value vectors.
                            for (int i = 0; i < KeyframeCount; i++)
                            {
                                keyframes[i].VectorValue = new();
                                for (int c = 0; c < ComponentCount; c++)
                                {
                                    var fullValue = DataStream.ReadSingleSlow();
                                    keyframes[i].VectorValue.Add(fullValue);
                                }
                            }

                            Keyframes = new(keyframes);
                        }
                        else
                        {
                            // These don't occur in the game.
                            throw new NotImplementedException();
                        }
                        break;
                    }
                case 1:
                    {
                        // The game doesn't support byte encoding, so we don't either.
                        throw new Exception($"Unsupported KeyframeSequence encoding value (1)");
                    }
                case 2:
                    {
                        // Encoded shorts.

                        VectorBias = new ();
                        VectorScale = new ();

                        for (int i = 0; i < ComponentCount; i++) VectorBias.Add(DataStream.ReadSingleSlow());
                        for (int i = 0; i < ComponentCount; i++) VectorScale.Add(DataStream.ReadSingleSlow());
                        for (int i = 0; i < ComponentCount; i++)
                        {
                            VectorScale[i] = (VectorScale[i] * 1.0f) / 65535.0f;  // TODO: does this *actually* need to happen?
                        }

                        if (mappableLayout)
                        {
                            var keyframes = new KeyframeData[KeyframeCount];
                            
                            // Allocate keyframe objects.
                            for (int i = 0; i < KeyframeCount; i++)
                            {
                                keyframes[i] = new ();
                            }

                            // Read times.
                            for (int i = 0; i < KeyframeCount; i++)
                            {
                                keyframes[i].Time = DataStream.ReadUnsigned32Little();
                            }

                            // Read value vectors.
                            for (int i = 0; i < KeyframeCount; i++)
                            {
                                keyframes[i].VectorValue = new();
                                for (int c = 0; c < ComponentCount; c++)
                                {
                                    var shortValue = (float)DataStream.ReadUnsigned16Little();

                                    // TODO: this conversion might actually be wrong!

                                    shortValue /= 65535.0f;
                                    shortValue *= VectorScale[c];
                                    shortValue += VectorBias[c];

                                    keyframes[i].VectorValue.Add(shortValue);
                                }
                            }

                            Keyframes = new (keyframes);
                        }
                        else
                        {
                            // These don't occur in the game.
                            throw new NotImplementedException();
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception($"Invalid KeyframeSequence encoding value (lower seven bits = {realEncodingByte})");
                    }
            }

            AssertEndStream();

            //for (int i = 0; i < KeyframeCount; i++)
            //{
            //    Debug.WriteLine($"Keyframe #{i} = at {Keyframes[i].Time} values are {String.Join(" ; ", Keyframes[i].VectorValue)}");
            //}
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
        public Int32 TexcoordArrayCount { get; set; }

        public class TexCoordInfo
        {
            public UInt32 TexCoords { get; set; }
            public Single[] TexCoordsBias { get; set; } = new Single[3];
            public Single TexCoordsScale { get; set; }
        }
        public List<TexCoordInfo> TexCoords { get; set; } = new ();

        public UInt32 Tangents { get; set; }
        public UInt32 Binormals { get; set; }

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

            bool wasNegative = false;
            if (TexcoordArrayCount < 0)
            {
                wasNegative = true;
                TexcoordArrayCount = -TexcoordArrayCount;  // YES I'M NOT KIDDING. YES IT'S HORRIBLE. YOU KNOW WHAT'S MORE HORRIBLE? THIS FUCKING ENGINE
            }

            if (DataStream.CanSeek && DataStream.Length - DataStream.Position != 28 && false)  // debug
            {
                var position = DataStream.Position;
                Debug.WriteLine($"{BitConverter.ToString(DataStream.ReadBytes((int)(DataStream.Length - DataStream.Position))).Replace('-', ' ')}");
                Debugger.Break();
                DataStream.Seek(position, SeekOrigin.Begin);
            }

            for (int j = 0; j < TexcoordArrayCount; j++)
            {
                TexCoordInfo tci = new ();
                tci.TexCoords = DataStream.ReadUnsigned32Little();
                for (int i = 0; i < 3; i++) tci.TexCoordsBias[i] = DataStream.ReadSingleSlow();
                tci.TexCoordsScale = DataStream.ReadSingleSlow();
                TexCoords.Add(tci);
            }

            if (wasNegative /* did they not care about this condition while serializing? */ || true)
            {
                Tangents = DataStream.ReadUnsigned32Little();
                Binormals = DataStream.ReadUnsigned32Little();
            }
            else
            {
                // something happens there as well
            }
            AssertEndStream();
        }
    }

    public class Unknown100 : Object3D
    {
        public static new ObjectType? Type => ObjectType.Unknown100;

        public uint IndexBuffer { get; set; }
        public uint Appearance { get; set; }

        public Unknown100(byte[] sourceData)
            : base(sourceData)
        {
            IndexBuffer = DataStream.ReadUnsigned32Little();
            Appearance = DataStream.ReadUnsigned32Little();

            Debug.WriteLine($"Read Unknown100: ib = {IndexBuffer}, appearance = {Appearance}");

            AssertEndStream();
        }
    }
    public class Unknown102 : Object3D
    {
        public static new ObjectType? Type => ObjectType.Unknown102;

        public uint IndexBuffer { get; set; }
        public uint Appearance { get; set; }

        public List<uint> VertexBuffersC { get; set; } = new ();

        public Unknown102(byte[] sourceData)
            : base(sourceData)
        {
            IndexBuffer = DataStream.ReadUnsigned32Little();
            Appearance = DataStream.ReadUnsigned32Little();

            var vertexBufferCount = DataStream.ReadSigned32Little();
            for (int i = 0; i < vertexBufferCount; i++)
            {
                VertexBuffersC.Add(DataStream.ReadUnsigned32Little());
            }

            Debug.WriteLine($"Read Unknown102: ib = {IndexBuffer}, appearance = {Appearance}, vertex buffer count = {VertexBuffersC.Count}");

            AssertEndStream();
        }
    }
    public class Unknown103 : Object3D
    {
        public static new ObjectType? Type => ObjectType.Unknown103;

        public uint IndexBuffer { get; set; }
        public uint Appearance { get; set; }

        public List<uint> VertexBuffersC { get; set; } = new();
        public List<uint> IndexBuffersD { get; set; } = new();

        public Unknown103(byte[] sourceData)
            : base(sourceData)
        {
            IndexBuffer = DataStream.ReadUnsigned32Little();
            Appearance = DataStream.ReadUnsigned32Little();

            var vertexBufferCount = DataStream.ReadSigned32Little();
            for (int i = 0; i < vertexBufferCount; i++)
            {
                VertexBuffersC.Add(DataStream.ReadUnsigned32Little());
            }

            var indexBufferCount = DataStream.ReadSigned32Little();  // ? LOD count ?
            for (int i = 0; i < indexBufferCount; i++)
            {
                IndexBuffersD.Add(DataStream.ReadUnsigned32Little());
            }

            Debug.WriteLine($"Read Unknown103: ib = {IndexBuffer}, appearance = {Appearance}, vertex buffer count = {VertexBuffersC.Count}, index buffer count = {IndexBuffersD.Count}");

            AssertEndStream();
        }
    }
    #endregion
}
