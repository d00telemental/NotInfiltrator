using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.StructBin
{
    public class EnumData
    {
        public int Id { get; private set; } = 0;
        public SemanticStructBin StructBin { get; private set; } = null;

        public UInt16 NameStrId { get; set; } = 0;
        public UInt32 ObjReference { get; set; } = 0;

        public string Name => StructBin.GetString(NameStrId);

        public EnumData(int id, SemanticStructBin sbin, Stream source)
        {
            Id = id;
            StructBin = sbin;

            NameStrId = source.ReadUnsigned16Little();
            Common.Assert(0 == source.ReadUnsigned16Little(), "StructBinEnum name padding is not padding");
            ObjReference = source.ReadUnsigned32Little();
        }
    }
}
