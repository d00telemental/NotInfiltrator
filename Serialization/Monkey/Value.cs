using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotInfiltrator.Serialization.Data;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Monkey
{
    public abstract class Value
    {
        public abstract FieldType Type { get; }
        public abstract Value ReadFromStream(Stream source, Object currentObject);

        public StructBin StructBin { get; set; }

        public static Value NewForType(StructBin sbin, FieldType type)
        {
            switch (type)
            {
                case FieldType.Int8:
                    return new Int8Value() { StructBin = sbin };
                case FieldType.UInt8:
                    return new UInt8Value() { StructBin = sbin };
                case FieldType.Int16:
                    return new Int16Value() { StructBin = sbin };
                case FieldType.UInt16:
                    return new UInt16Value() { StructBin = sbin };
                case FieldType.Int32:
                    return new Int32Value() { StructBin = sbin };
                case FieldType.UInt32:
                    return new UInt32Value() { StructBin = sbin };
                case FieldType.Int64:
                    return new Int64Value() { StructBin = sbin };
                case FieldType.UInt64:
                    return new UInt64Value() { StructBin = sbin };
                case FieldType.Reference:
                    return new ReferenceValue() { StructBin = sbin };
                default:
                    return null;
            }
        }
    }

    public abstract class ImplementedValue<T> : Value
    {
        public T Value { get; set; }
    }

    public class Int8Value : ImplementedValue<int>
    {
        public override FieldType Type => FieldType.Int8;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = source.ReadByte();
            return this;
        }
    }
    public class UInt8Value : ImplementedValue<uint>
    {
        public override FieldType Type => FieldType.UInt8;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = (uint)source.ReadByte();
            return this;
        }
    }
    public class Int16Value : ImplementedValue<int>
    {
        public override FieldType Type => FieldType.Int16;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = source.ReadSigned16Little();
            return this;
        }
    }
    public class UInt16Value : ImplementedValue<uint>
    {
        public override FieldType Type => FieldType.UInt16;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = source.ReadUnsigned16Little();
            return this;
        }
    }
    public class Int32Value : ImplementedValue<int>
    {
        public override FieldType Type => FieldType.Int32;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = source.ReadSigned32Little();
            return this;
        }
    }
    public class UInt32Value : ImplementedValue<uint>
    {
        public override FieldType Type => FieldType.UInt32;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = source.ReadUnsigned32Little();
            return this;
        }
    }
    public class Int64Value : ImplementedValue<long>
    {
        public override FieldType Type => FieldType.Int64;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = source.ReadSigned64Little();
            return this;
        }
    }
    public class UInt64Value : ImplementedValue<ulong>
    {
        public override FieldType Type => FieldType.UInt64;
        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            Value = source.ReadUnsigned64Little();
            return this;
        }
    }
    //public class BooleanValue : ImplementedValue<bool>
    //{
    //    public override FieldType Type => FieldType.Boolean;
    //}
    //public class FloatValue : ImplementedValue<float>
    //{
    //    public override FieldType Type => FieldType.Float;
    //}
    //public class DoubleValue : ImplementedValue<double>
    //{
    //    public override FieldType Type => FieldType.Double;
    //}
    //public class CharValue : ImplementedValue<char>
    //{
    //    public override FieldType Type => FieldType.Char;
    //}
    //public class StringValue : ImplementedValue<StringData>
    //{
    //    public override FieldType Type => FieldType.String;
    //}
    //public class PodValue : ImplementedValue<object>
    //{
    //    public override FieldType Type => FieldType.POD;
    //}

    public class ReferenceValue : ImplementedValue<Object>
    {
        public override FieldType Type => FieldType.Reference;

        public int RefId { get; set; }
        public bool IsNil { get; set; } = false;

        public override Value ReadFromStream(Stream source, Object currentObject)
        {
            RefId = source.ReadSigned32Little();

            IsNil = RefId == -1;
            Value = !IsNil ? Object.ParseTree(StructBin, RefId, currentObject) : null;

            return this;
        }
    }
}
