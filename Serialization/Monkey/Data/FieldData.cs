using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Serialization.Monkey;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Monkey.Data
{
    public class FieldData : Data
    {
        public UInt16 NameId { get; init; } = 0;
        public FieldType Type { get; init; } = 0;
        public UInt16 Offset { get; init; } = 0;
        public UInt16 ChildKind { get; init; } = 0;

        public string Name => StructBin.GetString(NameId);
        public string TypeName => GetTypeName(Type);
        public int Size => GetTypeSize(Type);
        public int OffsetToEnd => Offset + Size;

        public FieldData(int id, StructBin sbin) : base(id, sbin) { }

        public int GetTypeSize(FieldType fieldType)
            => fieldType switch
            {
                FieldType.Int8 => 1,
                FieldType.UInt8 => 1,
                FieldType.Int16 => 2,
                FieldType.UInt16 => 2,
                FieldType.Int32 => 4,
                FieldType.UInt32 => 4,
                FieldType.Int64 => 8,
                FieldType.UInt64 => 8,
                FieldType.Boolean => 1,
                FieldType.Float => 4,
                FieldType.Double => 8,
                FieldType.Char => 2,
                FieldType.String => 2,
                FieldType.POD => -1,  // TODO
                FieldType.Reference => 4,
                FieldType.InlineStruct => StructBin.StructDatas[ChildKind].Size,
                FieldType.Array => 4,
                FieldType.Enum => 4,
                FieldType.Bitfield => 4,
                FieldType.Symbol => 2,
                FieldType.Unknown15 => 2,
                FieldType.Unknown16 => 4,
                _ => throw new Exception("Unknown field type")
            };

        public static string GetTypeName(FieldType fieldType)
            => (ushort)fieldType switch
            {
                var t when t is > 0 and <= 0x16 => Enum.GetName(typeof(FieldType), fieldType),
                _ => $"N/A: {fieldType}"
            };
    }
}
