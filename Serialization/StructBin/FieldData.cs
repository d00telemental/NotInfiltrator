using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.StructBin
{
    public class FieldData
    {
        public int Id { get; set; } = 0;

        public UInt16 NameStrId { get; set; } = 0;
        public UInt16 Type { get; set; } = 0;
        public UInt16 Offset { get; set; } = 0;
        public UInt16 ChildKind { get; set; } = 0;

        public FieldData(Stream source)
        {
            NameStrId = source.ReadUnsigned16Little();
            Type = source.ReadUnsigned16Little();
            Offset = source.ReadUnsigned16Little();
            ChildKind = source.ReadUnsigned16Little();
        }

        public int Size => GetSize(Type);
        public string TypeName => GetTypeName(Type);

        public static int GetSize(int fieldType)
        {
            return fieldType switch
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
        }

        public static string GetTypeName(int fieldType)
        {
            return fieldType switch
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
}
