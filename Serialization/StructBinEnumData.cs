using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization
{
    public class StructBinEnumData
    {
        public int Id { get; set; } = 0;

        public UInt16 NameStrId { get; set; } = 0;
        public UInt32 ObjReference { get; set; } = 0;

        public StructBinEnumData(Stream source)
        {
            NameStrId = source.ReadUnsigned16Little();
            Common.Assert(0 == source.ReadUnsigned16Little(), "StructBinEnum name padding is not padding");
            ObjReference = source.ReadUnsigned32Little();
        }
    }
}
