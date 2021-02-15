using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class EnumData : Data
    {
        public UInt16 NameStrId { get; set; } = 0;
        public UInt32 ObjReference { get; set; } = 0;

        public string Name => StructBin.GetString(NameStrId);

        public EnumData(int id, StructBin sbin, Stream source)
            : base(id, sbin)
        {
            NameStrId = source.ReadUnsigned16Little();
            Common.Assert(0 == source.ReadUnsigned16Little(), "StructBinEnum name padding is not padding");
            ObjReference = source.ReadUnsigned32Little();
        }
    }
}
