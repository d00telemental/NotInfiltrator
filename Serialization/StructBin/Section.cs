using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.StructBin
{
    public class Section
    {
        public long Start { get; set; } = 0;
        public long End { get; set; } = 0;

        public string Label { get; set; } = null;
        public Int32 DataLength { get; set; } = 0;
        public Int32 Hash { get; set; } = 0;
        public byte[] Data { get; set; } = null;

        public Int32 AlignedDataLength
            => Label == "STRU" ? GetNextMultipleOfFour(DataLength) : DataLength;

        public static Section Read(Stream stream)
        {
            var entry = new Section { };
            entry.Start = stream.Position;
            entry.Label = Encoding.ASCII.GetString(stream.ReadBytes(4));
            entry.DataLength = stream.ReadSigned32Little();
            entry.Hash = stream.ReadSigned32Little();
            entry.Data = stream.ReadBytes(entry.AlignedDataLength);
            entry.End = stream.Position;
            return entry;
        }

        // https://stackoverflow.com/a/2022194
        private static int GetNextMultipleOfFour(int num)
            => (num + 3) & ~3;
    }

    public static class SectionExtensions
    {
        public static MemoryStream NewMemoryStream(this Section section)
            => new MemoryStream(section.Data, 0, section.DataLength);
    }
}
