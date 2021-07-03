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
        Background = 4,            //   unused
        Camera = 5,                //   unused
        CompositingMode = 6,       // read
        Fog = 7,                   //   unused
        PolygonMode = 8,           // read
        Group = 9,                 // read
        Image2D = 10,              // read
        TriangleStripArray = 11,   //   unused
        Light = 12,                //   unused
        Material = 13,             //   unused
        Mesh = 14,                 // read
        MorphingMesh = 15,         //   unused
        SkinnedMesh = 16,          // read
        Texture2D = 17,            // read
        Sprite = 18,               //   unused
        KeyframeSequence = 19,
        VertexArray = 20,          // read
        VertexBuffer = 21,         // read
        World = 22,                //   unused
        /**/ Unknown100 = 100,     //     ? = submesh
        /**/ Unknown101 = 101,     //     ? = triangle list / index buffer
        /**/ Unknown102 = 102,     //     ? = submesh
        /**/ Unknown103 = 103,     //     ? = submesh
        ExternalReference = 255,   //   unused
    }
}
