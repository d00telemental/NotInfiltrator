using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotInfiltrator.Serialization.Monkey.Data;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Monkey
{
    public abstract class Value
    {
        public abstract FieldType Type { get; }
        public abstract Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED);

        public StructBin StructBin { get; set; }

        public static Value NewForType(StructBin sbin, FieldType type)
            => type switch
        {
            FieldType.Int8 => new Int8Value() { StructBin = sbin },
            FieldType.UInt8 => new UInt8Value() { StructBin = sbin },
            FieldType.Int16 => new Int16Value() { StructBin = sbin },
            FieldType.UInt16 => new UInt16Value() { StructBin = sbin },
            FieldType.Int32 => new Int32Value() { StructBin = sbin },
            FieldType.UInt32 => new UInt32Value() { StructBin = sbin },
            FieldType.Int64 => new Int64Value() { StructBin = sbin },
            FieldType.UInt64 => new UInt64Value() { StructBin = sbin },
            FieldType.Boolean => new BooleanValue() { StructBin = sbin },
            FieldType.Float => new FloatValue() { StructBin = sbin },
            FieldType.Double => new DoubleValue() { StructBin = sbin },
            FieldType.Char => throw new NotImplementedException(),
            FieldType.String => new StringValue() { StructBin = sbin },
            FieldType.POD => throw new NotImplementedException(),
            FieldType.Reference => new ReferenceValue() { StructBin = sbin },
            FieldType.InlineStruct => new InlineStructValue() { StructBin = sbin },
            FieldType.Array => new ArrayValue() { StructBin = sbin },
            FieldType.Enum => new EnumValue() { StructBin = sbin },
            FieldType.Bitfield => throw new NotImplementedException(),
            FieldType.Symbol => new SymbolValue() { StructBin = sbin },
            FieldType.Unknown15 => throw new NotImplementedException(),
            FieldType.Unknown16 => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }

    public abstract class ImplementedValue<T> : Value
    {
        public T Value { get; set; }
    }

    public class Int8Value : ImplementedValue<int>
    {
        public override FieldType Type => FieldType.Int8;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadByte();
            return this;
        }
    }
    public class UInt8Value : ImplementedValue<uint>
    {
        public override FieldType Type => FieldType.UInt8;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = (uint)source.ReadByte();
            return this;
        }
    }
    public class Int16Value : ImplementedValue<int>
    {
        public override FieldType Type => FieldType.Int16;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadSigned16Little();
            return this;
        }
    }
    public class UInt16Value : ImplementedValue<uint>
    {
        public override FieldType Type => FieldType.UInt16;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadUnsigned16Little();
            return this;
        }
    }
    public class Int32Value : ImplementedValue<int>
    {
        public override FieldType Type => FieldType.Int32;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadSigned32Little();
            return this;
        }
    }
    public class UInt32Value : ImplementedValue<uint>
    {
        public override FieldType Type => FieldType.UInt32;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadUnsigned32Little();
            return this;
        }
    }
    public class Int64Value : ImplementedValue<long>
    {
        public override FieldType Type => FieldType.Int64;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadSigned64Little();
            return this;
        }
    }
    public class UInt64Value : ImplementedValue<ulong>
    {
        public override FieldType Type => FieldType.UInt64;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadUnsigned64Little();
            return this;
        }
    }
    public class BooleanValue : ImplementedValue<bool>
    {
        public override FieldType Type => FieldType.Boolean;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadByte() != 0;
            return this;
        }
    }
    public class FloatValue : ImplementedValue<float>
    {
        public override FieldType Type => FieldType.Float;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = BitConverter.ToSingle(source.ReadBytes(4), 0);
            return this;
        }
    }
    public class DoubleValue : ImplementedValue<double>
    {
        public override FieldType Type => FieldType.Double;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = BitConverter.ToDouble(source.ReadBytes(8), 0);
            return this;
        }
    }
    //public class CharValue : ImplementedValue<char>
    //{
    //    public override FieldType Type => FieldType.Char;
    //}
    public class StringValue : ImplementedValue<StringData>
    {
        public override FieldType Type => FieldType.String;
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            int strId = source.ReadSigned16Little();
            Value = StructBin.StringDatas[strId];
            return this;
        }
    }
    //public class PodValue : ImplementedValue<object>
    //{
    //    public override FieldType Type => FieldType.POD;
    //}
    public class ReferenceValue : ImplementedValue<Object>
    {
        public override FieldType Type => FieldType.Reference;

        public int RefId { get; set; }
        public bool IsNil { get; set; } = false;

        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            RefId = source.ReadSigned32Little();

            IsNil = RefId == -1;
            Value = !IsNil ? Object.ParseTree(StructBin, RefId, currentObject) : null;

            return this;
        }
    }
    public class InlineStructValue : ImplementedValue<StructObject>
    {
        public override FieldType Type => FieldType.InlineStruct;

        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = currentSD != null
                ? new StructObject(StructBin, -1, null, source, currentSD.Id)
                : new StructObject(StructBin, -1, null, source);
            return this;
        }
    }
    public class ArrayValue : ReferenceValue
    {
        public override FieldType Type => FieldType.Array;

        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            RefId = source.ReadSigned32Little();

            IsNil = RefId == -1;
            Value = !IsNil ? Object.ParseTree(StructBin, RefId, currentObject) : null;

            return this;
        }
    }
    public class EnumValue : ImplementedValue<Tuple<EnumData, int>>
    {
        public override FieldType Type => FieldType.Enum;

        public EnumData ValueMeta => Value.Item1;
        public int ValueId => Value.Item2;

        public object ValueStringData => ValueMeta.Object.Items[ValueId];

        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = new(null, source.ReadSigned32Little());
            return this;
        }
    }
    //public class BitfieldValue : ImplementedValue<object>
    //{
    //    public override FieldType Type => FieldType.Bitfield;
    //}
    public class SymbolValue : ImplementedValue<short>
    {
        public override FieldType Type => FieldType.Symbol;
        
        public override Value ReadFromStream(Stream source, Object currentObject, FieldData currentFD, StructData currentSD, EnumData currentED)
        {
            Value = source.ReadSigned16Little();
            return this;
        }
    }
}
