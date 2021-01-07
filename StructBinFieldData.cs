using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NotInfiltrator
{
    public class StructBinFieldData
    {
        public UInt16 NameStrId { get; set; } = 0;
        public UInt16 Type { get; set; } = 0;
        public UInt16 Offset { get; set; } = 0;
        public UInt16 Unknown { get; set; } = 0;

        public StructBinFieldData(Stream source)
        {
            NameStrId = source.ReadUnsigned16Little();
            Type = source.ReadUnsigned16Little();
            Offset = source.ReadUnsigned16Little();
            Unknown = source.ReadUnsigned16Little();
        }
    }
}
