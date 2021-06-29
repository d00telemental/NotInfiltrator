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
            => UncompressedLength - sizeof(CompressionScheme) - sizeof(UInt32) - sizeof(UInt32) - sizeof(UInt32);

        public Section(Stream stream)
        {
            Compression = (CompressionScheme)stream.ReadByte();
            Common.AssertEquals((int)Compression, (int)CompressionScheme.Uncompressed, "Compressed sections are not supported yet");

            TotalSectionLength = stream.ReadUnsigned32Little();
            UncompressedLength = stream.ReadUnsigned32Little();
            ReadingStream = new MemoryStream(stream.ReadBytes((int)ObjectsOnlyUncompressedLength));
            AdlerChecksum = stream.ReadUnsigned32Little();

            var implementedObjectTypes = new byte[] { 0, 1, 2, 3, 6, 8, 9, 10, 14, 17, 20, 21, /**/ 100, 101 /**/ };
            var objectEnumerator = new AgnosticObjectEnumerator(ReadingStream);
            var objectInfos = objectEnumerator.ReadAll();

            objectEnumerator
                .AllMetTypes()
                .Except(implementedObjectTypes)
                .OrderBy(t => t)
                .ToList()
                .ForEach(t => Debug.WriteLine(t));

            foreach (
                var parsedObject in
                from info in objectInfos
                where implementedObjectTypes.Contains(info.Type)
                select Object.Read(info)
            )
            {
                Debug.WriteLine((parsedObject as Object3D)?.UserParameters.Count);
            }
        }
    }
}
