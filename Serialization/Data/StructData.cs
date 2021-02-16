using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class StructData : Data
    {
        public UInt16 NameId { get; init; } = 0;
        public UInt16 FirstFieldId { get; init; } = 0;
        public UInt16 FieldCount { get; init; } = 0;

        public string Name => StructBin.GetString(NameId);
        public List<FieldData> Fields => StructBin.FieldDatas.GetRange(FirstFieldId, FieldCount);
        public int Size => Fields.DefaultIfEmpty().Last()?.OffsetToEnd ?? 0;  // TODO: account for the last field alignment

        public StructData(int id, StructBin sbin) : base(id, sbin) { }
    }
}
