﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Nokia
{
    public class Section
    {
        public CompressionScheme Compression { get; set; }
        public UInt32 TotalSectionLength { get; set; }
        public UInt32 UncompressedLength { get; set; }
        public List<Nokia.Object> Objects { get; set; } = new List<Object>();
        public UInt32 AdlerChecksum { get; set; }

        public UInt32 ObjectsOnlyUncompressedLength => UncompressedLength - sizeof(CompressionScheme) - sizeof(UInt32) - sizeof(UInt32) - sizeof(UInt32);

        public Section(Stream stream)
        {
            Compression = (CompressionScheme)stream.ReadByte();
            Common.AssertEquals((int)Compression, (int)CompressionScheme.Uncompressed, "Compressed sections are not supported yet");

            TotalSectionLength = stream.ReadUnsigned32Little();
            UncompressedLength = stream.ReadUnsigned32Little();

            var rawData = stream.ReadBytes((int)ObjectsOnlyUncompressedLength);
            AdlerChecksum = stream.ReadUnsigned32Little();

            var rawDataStream = new MemoryStream(rawData);
            while (rawDataStream.Position < rawDataStream.Length)
            {
                Objects.Add(Object.ReadFromStream(rawDataStream));
            }
        }
    }
}