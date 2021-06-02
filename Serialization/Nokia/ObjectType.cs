using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Nokia
{
    public enum ObjectType : byte
    {
        Header = 0,
        AnimationController = 1,
        AnimationTrack = 2,
        Appearance = 3,
        Background = 4,
        Camera = 5,
        CompositingMode = 6,
        Fog = 7,
        PolygonMode = 8,
        Group = 9,
        Image2D = 10,
        TriangleStripArray = 11,
        Light = 12,
        Material = 13,
        Mesh = 14,
        MorphingMesh = 15,
        SkinnedMesh = 16,
        Texture2D = 17,
        Sprite = 18,
        KeyframeSequence = 19,
        VertexArray = 20,
        VertexBuffer = 21,
        World = 22,
        /**/ Unknown100 = 100,
        /**/ Unknown101 = 101,
        ExternalReference = 255
    }
}
