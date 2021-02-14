using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.StructBin
{
    public class StructData
    {
        public int Id { get; set; } = 0;


        public UInt16 NameStrId { get; set; } = 0;
        public UInt16 FirstFieldId { get; set; } = 0;
        public UInt16 FieldCount { get; set; } = 0;

        public StructData(Stream source)
        {
            NameStrId = source.ReadUnsigned16Little();
            FirstFieldId = source.ReadUnsigned16Little();
            FieldCount = source.ReadUnsigned16Little();
        }
    }
}
