using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.StructBin
{
    public class Section
    {
        public long Start;
        public long End;

        public string Label;
        public Int32 DataLength;
        public Int32 Hash;
        public byte[] Data;

        // https://stackoverflow.com/a/2022194
        private int GetNextMultipleOfFour(int num)
            => (num + 3) & ~3;

        public Int32 RealDataLength
            => Label == "STRU" ? GetNextMultipleOfFour(DataLength) : DataLength;

        public static Section Read(Stream stream)
        {
            var entry = new Section { };
            entry.Start = stream.Position;
            entry.Label = Encoding.ASCII.GetString(stream.ReadBytes(4));
            entry.DataLength = stream.ReadSigned32Little();
            entry.Hash = stream.ReadSigned32Little();
            entry.Data = stream.ReadBytes(entry.RealDataLength);
            entry.End = stream.Position;
            return entry;
        }
    }

    public static class SectionExtensions
    {
        public static MemoryStream NewMemoryStream(this Section section)
            => new MemoryStream(section.Data, 0, section.DataLength);
    }
}
