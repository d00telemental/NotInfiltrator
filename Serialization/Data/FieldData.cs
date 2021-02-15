using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class FieldData : Data
    {
        public UInt16 NameId { get; init; } = 0;
        public UInt16 Type { get; init; } = 0;
        public UInt16 Offset { get; init; } = 0;
        public UInt16 ChildKind { get; init; } = 0;

        public string Name => StructBin.GetString(NameId);
        public string TypeName => GetTypeName(Type);
        public int Size => GetTypeSize(Type);

        public FieldData(int id, StructBin sbin) : base(id, sbin) { }

        public static int GetTypeSize(int fieldType)
            => fieldType switch
            {
                0x01 => 1,  // [PrimitiveType] Int8
                0x02 => 1,  // [PrimitiveType] UInt8
                0x03 => 2,  // [PrimitiveType] Int16
                0x04 => 2,  // [PrimitiveType] UInt16
                0x05 => 4,  // [PrimitiveType] Int32
                0x06 => 4,  // [PrimitiveType] UInt32
                0x07 => 8,  // [PrimitiveType] Int64
                0x08 => 8,  // [PrimitiveType] UInt64
                0x09 => 1,  // [PrimitiveType] Boolean
                0x0A => 4,  // [PrimitiveType] Float
                0x0B => 8,  // [PrimitiveType] Double
                0x0C => 2,  // [PrimitiveType] Char
                0x0D => 2,  // [PrimitiveType] String
                0x0E => -1, // [PrimitiveType] POD             // TODO
                0x0F => 4,  // [PrimitiveType] Reference
                0x10 => -1, // [PrimitiveType] InlineStruct    // TODO
                0x11 => 4,  // [PrimitiveType] Array
                0x12 => 4,  // [PrimitiveType] Enum
                0x13 => 4,  // [PrimitiveType] BitField
                0x14 => 2,  // [PrimitiveType] Symbol
                0x15 => 2,
                0x16 => 4,
                _ => throw new Exception("Unknown field type")
            };

        public static string GetTypeName(int fieldType)
            => fieldType switch
            {
                0x01 => "Int8",
                0x02 => "UInt8",
                0x03 => "Int16",
                0x04 => "UInt16",
                0x05 => "Int32",
                0x06 => "UInt32",
                0x07 => "Int64",
                0x08 => "UInt64",
                0x09 => "Boolean",
                0x0A => "Float",
                0x0B => "Double",
                0x0C => "String",
                0x0D => "String",
                0x0E => "POD",
                0x0F => "Reference",
                0x10 => "InlineStruct",
                0x11 => "Array",
                0x12 => "Enum",
                0x13 => "BitField",
                0x14 => "Symbol",
                0x15 => null,
                0x16 => null,
                _ => throw new Exception("Unknown field type")
            };
    }
}
