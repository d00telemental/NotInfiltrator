using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NotInfiltrator
{
    public class StructBinFieldData
    {
        public int Id { get; set; } = 0;

        public UInt16 NameStrId { get; set; } = 0;
        public UInt16 Type { get; set; } = 0;
        public UInt16 Offset { get; set; } = 0;
        public UInt16 Unknown { get; set; } = 0;

        public StructBinFieldData(Stream source)
        {
            NameStrId = source.ReadUnsigned16Little();
            Type = source.ReadUnsigned16Little();
            Offset = source.ReadUnsigned16Little();
            Unknown = source.ReadUnsigned16Little();
        }

        public int Size => GetSize(Type);

        public static int GetSize(int fieldType)
        {
            return fieldType switch
            {
                0x01 => 1,
                0x02 => 1,
                0x03 => 2,
                0x04 => 2,
                0x05 => 4,
                0x06 => 4,
                0x07 => 8,
                0x08 => 8,
                0x09 => 1,
                0x0A => 4,
                0x0B => 8,
                0x0C => 2,
                0x0D => 2,
                0x0E => -1,  // TODO
                0x0F => 4,
                0x10 => -1,  // TODO
                0x11 => 4,
                0x12 => 4,
                0x13 => 4,
                0x14 => 2,
                0x15 => 2,
                0x16 => 4,
                _ => 0
            };
        }
    }
}
