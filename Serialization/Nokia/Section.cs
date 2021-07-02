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
        protected Stream ReadingStream { get; set; }

        public CompressionScheme Compression { get; set; }
        public UInt32 TotalSectionLength { get; set; }
        public UInt32 UncompressedLength { get; set; }
        public List<Nokia.Object> Objects { get; set; } = new List<Object>();
        public UInt32 AdlerChecksum { get; set; }

        public UInt32 ObjectsOnlyUncompressedLength
            => TotalSectionLength - sizeof(CompressionScheme) - sizeof(UInt32) - sizeof(UInt32) - sizeof(UInt32);

        public Section(Stream stream)
        {
            Compression = (CompressionScheme)stream.ReadByte();
            Common.AssertEquals((int)Compression, (int)CompressionScheme.Uncompressed, "Compressed sections are not supported yet");

            TotalSectionLength = stream.ReadUnsigned32Little();
            UncompressedLength = stream.ReadUnsigned32Little();
            ReadingStream = new MemoryStream(stream.ReadBytes((int)ObjectsOnlyUncompressedLength));
            AdlerChecksum = stream.ReadUnsigned32Little();

            var implementedObjectTypes = new byte[] { 0, 1, 2, 3, 6, 8, 9, 10, 14, 16, 17, 20, 21, /**/ 100, 101 /**/ };
            var objectEnumerator = new AgnosticObjectEnumerator(ReadingStream);
            var objectInfos = objectEnumerator.ReadAll();

            objectInfos.Where(oi => implementedObjectTypes.Contains(oi.Type))
                .ToList()
                .ForEach(oi =>
                {
                    var obj = Object.Read(oi); /*Debug.WriteLine($"ObjectInfo @ {oi.StartOffset}, type = {{ {oi.Type} / {obj.Type} }}");*/
                });

            //objectInfos.Where(oi => !implementedObjectTypes.Contains(oi.Type))
            //    .OrderBy(oi => oi.Type)
            //    .GroupBy(oi => oi.Type).Select(goi => goi.First())
            //    .ToList()
            //    .ForEach((aoi) => Debug.WriteLine($"{aoi.Type}"));
        }
    }
}
