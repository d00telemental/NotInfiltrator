using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Data
{
    public class ObjectData : Data
    {
        public byte[] EncodedOffset { get; private set; } = null;
        public int AlignedLength { get; set; } = 0;

        public int Offset { get; private set; } = 0;

        public byte[] AlignedData => StructBin.Sections["DATA"].Data[Offset..(Offset + AlignedLength)];

        public ObjectData(int id, StructBin sbin, int offset, byte[] encodedOffset)
            : base(id, sbin)
        {
            Offset = offset;
            EncodedOffset = encodedOffset;
        }
    }
}
