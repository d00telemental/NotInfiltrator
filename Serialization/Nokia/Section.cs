using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Nokia
{
    public class Section
    {
        protected byte[] ObjectBytes { get; set; }
        protected Stream ReadingStream { get; set; }

        public CompressionScheme Compression { get; set; }
        public UInt32 TotalSectionLength { get; set; }
        public UInt32 UncompressedLength { get; set; }
        public List<Nokia.Object> Objects { get; set; } = new ();
        public UInt32 AdlerChecksum { get; set; }

        public int ObjectsLength
            => (int)TotalSectionLength - sizeof(CompressionScheme) - sizeof(UInt32) - sizeof(UInt32) - sizeof(UInt32);

        public Section(Stream stream)
        {
            Compression = (CompressionScheme)stream.ReadByte();
            Common.AssertEquals((int)Compression, (int)CompressionScheme.Uncompressed, "Compressed sections are not supported yet");

            TotalSectionLength = stream.ReadUnsigned32Little();
            UncompressedLength = stream.ReadUnsigned32Little();
            ObjectBytes = stream.ReadBytes(ObjectsLength);
            AdlerChecksum = stream.ReadUnsigned32Little();

            ReadingStream = new MemoryStream(ObjectBytes);

            var implementedTypes = new byte[] { /* 0, 1, 2, 3, 6, 8, 9, 10, 14, 16, 17, 19, 20, 21, */ 103 };
            var objectEnumerator = new AgnosticObjectEnumerator(ReadingStream);

            foreach (var info in 
                from info in objectEnumerator.Read()
                where implementedTypes.Contains(info.Type)
                select info)
            {
                var readObject = Object.Read(info);
                Objects.Add(readObject);
            }
        }
    }
}
