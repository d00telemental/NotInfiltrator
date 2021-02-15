using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class EnumData : Data
    {
        public UInt16 NameId { get; init; } = 0;
        public UInt32 ObjReference { get; init; } = 0;

        public string Name => StructBin.GetString(NameId);

        public EnumData(int id, StructBin sbin) : base(id, sbin) { }
    }
}
