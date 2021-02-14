﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class SectionData
    {
        public int Id { get; private set; } = 0;
        public StructBin StructBin { get; private set; } = null;

        public long Start { get; private set; } = 0;
        public long End { get; private set; } = 0;

        public string Label { get; private set; } = null;
        public Int32 DataLength { get; private set; } = 0;
        public Int32 Hash { get; private set; } = 0;
        public byte[] Data { get; private set; } = null;

        public Int32 AlignedDataLength
            => Label == "STRU" ? NextMultipleOfFour(DataLength) : DataLength;

        public SectionData(int id, StructBin sbin, Stream source)
        {
            Id = id;
            StructBin = sbin;

            Start = source.Position;

            Label = Encoding.ASCII.GetString(source.ReadBytes(4));
            DataLength = source.ReadSigned32Little();
            Hash = source.ReadSigned32Little();
            Data = source.ReadBytes(AlignedDataLength);

            End = source.Position;
        }

        // https://stackoverflow.com/a/2022194
        private static int NextMultipleOfFour(int num)
            => (num + 3) & ~3;
    }

    public static class SectionExtensions
    {
        public static MemoryStream NewMemoryStream(this SectionData section)
            => new MemoryStream(section.Data, 0, section.DataLength);
    }
}