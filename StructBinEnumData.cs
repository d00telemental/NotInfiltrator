using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NotInfiltrator
{
    public class StructBinEnumData
    {
        public int Id { get; set; } = 0;

        public UInt16 NameStrId { get; set; } = 0;
        public UInt32 Unknown { get; set; } = 0;

        public StructBinEnumData(Stream source)
        {
            NameStrId = source.ReadUnsigned16Little();
            Common.Assert(0 == source.ReadUnsigned16Little(), "StructBinEnum name padding is not padding");
            Unknown = source.ReadUnsigned32Little();
        }
    }
}
