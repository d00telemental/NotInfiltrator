using NotInfiltrator.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Data
{
    public class ObjectData : Data
    {
        public int AlignedLength { get; init; } = 0;
        public byte[] AlignedData { get; init; } = null;
        public byte[] EncodedOffset { get; init; } = null;  // keeping because there's some kind of check the game does with it

        public int Offset { get; init; } = 0;

        public ObjectData(int id, StructBin sbin) : base(id, sbin) { }

        public static int DecodeOffset(byte[] b)
        {
            if (b is null || b.Length != 4)
            {
                throw new NullReferenceException();
            }

            UInt32 res = 0;
            res |= (UInt32)((UInt32)b[0] >> (Int32)0x3);
            res |= (UInt32)((UInt32)b[1] << (Int32)0x5);
            res |= (UInt32)((UInt32)b[2] << (Int32)0xD);
            res |= (UInt32)((UInt32)b[3] << (Int32)0x15);

            Common.Assert(res <= int.MaxValue, "Decoded object offset is bigger than max. possible Int32");
            return (int)res;
        }
    }
}
