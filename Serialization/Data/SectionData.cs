using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class SectionData : Data
    {
        public static readonly int HeaderSize = 4 * 3;

        public long Start { get; init; } = 0;
        public long End { get; init; } = 0;
        public long AlignedEnd { get; init; } = 0;

        public string Label { get; init; } = null;
        public Int32 DataLength { get; init; } = 0;
        public Int32 Hash { get; init; } = 0;
        public byte[] Data { get; init; } = null;

        public Int32 AlignedDataLength => GetAlignedDataLength(Label, DataLength);
        public long DataOffset => Start + HeaderSize;

        public SectionData(int id, StructBin sbin) : base(id, sbin) { }

        public static int GetAlignedDataLength(string label, int dataLength)
            => label == "STRU" ? Common.NextMultipleOfFour(dataLength) : dataLength;
    }
}
