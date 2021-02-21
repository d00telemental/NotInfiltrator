using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Monkey
{
    public enum FieldType : ushort
    {
        Int8 = 0x1,
        UInt8 = 0x2,
        Int16 = 0x3,
        UInt16 = 0x4,
        Int32 = 0x5,
        UInt32 = 0x6,
        Int64 = 0x7,
        UInt64 = 0x8,
        Boolean = 0x9,
        Float = 0xA,
        Double = 0xB,
        Char = 0xC,
        String = 0xD,
        POD = 0xE,
        Reference = 0xF,
        InlineStruct = 0x10,
        Array = 0x11,
        Enum = 0x12,
        Bitfield = 0x13,
        Symbol = 0x14,
        Unknown15 = 0x15,
        Unknown16 = 0x16
    }
}
