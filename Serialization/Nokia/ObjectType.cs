using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Nokia
{
    public enum ObjectType : byte
    {
        Header = 0,                // read
        AnimationController = 1,   // read
        AnimationTrack = 2,        // read
        Appearance = 3,            // read
        Background = 4,
        Camera = 5,
        CompositingMode = 6,       // read
        Fog = 7,
        PolygonMode = 8,           // read
        Group = 9,                 // read
        Image2D = 10,              // read
        TriangleStripArray = 11,
        Light = 12,
        Material = 13,
        Mesh = 14,                 // read
        MorphingMesh = 15,
        SkinnedMesh = 16,
        Texture2D = 17,            // read
        Sprite = 18,
        KeyframeSequence = 19,
        VertexArray = 20,          // read
        VertexBuffer = 21,         // read
        World = 22,
        /**/ Unknown100 = 100,
        /**/ Unknown101 = 101,
        ExternalReference = 255
    }
}
