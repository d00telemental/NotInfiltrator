using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotInfiltrator.Serialization.Monkey.Data;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Monkey
{

    public abstract class Object
    {
        public StructBin StructBin { get; } = null;
        public int Id { get; } = 0;
        public Object Parent { get; } = null;

        protected Object(StructBin sbin, int id, Object parent)
        {
            StructBin = sbin;
            Id = id;
            Parent = parent;
        }

        public static Object ParseTree(StructBin sbin, int id, Object parent)
        {
            var objData = sbin.ObjectDatas[id];
            var objDataStream = new MemoryStream(objData.AlignedData);
            switch (objData.ObjectDefinitionSize / 4)
            {
                case 0:
                    return new StructObject(sbin, id, parent, objDataStream);
                case 1:
                    return new UnstructuredObject(sbin, id, parent, objDataStream);
                case 2:
                    return new ArrayObject(sbin, id, parent, objDataStream);
                default:
                    Debug.WriteLine($"Invalid ODS encountered!");
                    throw new Exception("Invalid ODS encountered!");
            }
        }
    }

    public class StructObjectField
    {
        public FieldData MetaData { get; set; }
        public Value Value { get; set; }
    }

    public class StructObject : Object
    {
        public StructData StructData { get; set; }
        public List<StructObjectField> Fields { get; set; } = new();

        public StructObject(StructBin sbin, int id, Object parent, Stream source, int structDataId)
            : base(sbin, id, parent)
        {
            StructData = sbin.StructDatas[structDataId];

            long structStartOffset = source.Position;

            foreach (var fieldData in StructData.Fields)
            {
                source.Seek(structStartOffset + fieldData.Offset, SeekOrigin.Begin);

                var value = fieldData.Type switch
                {
                    FieldType.InlineStruct
                        => Value.NewForType(sbin, fieldData.Type).ReadFromStream(source, this, null, sbin.StructDatas[fieldData.ChildKind], null),
                    _
                        => Value.NewForType(sbin, fieldData.Type).ReadFromStream(source, this, null, null, null)
                };

                if (value is EnumValue valueAsEnum)
                {
                    valueAsEnum.Value = new(sbin.EnumDatas[fieldData.ChildKind], valueAsEnum.ValueId);
                }

                Fields.Add(new() { MetaData = fieldData, Value = value });
            }
        }

        public StructObject(StructBin sbin, int id, Object parent, Stream source)
            : this(sbin, id, parent, source, source.ReadSigned16Little())
        {

        }
    }

    public class UnstructuredObjectEntry
    {
        public StringData Name { get; set; }
        public FieldType Type { get; set; }
        public int InnerOffset { get; set; }
        public Value Value { get; set; }
    }

    public class UnstructuredObject : Object
    {
        public short Size { get; set; }
        public List<UnstructuredObjectEntry> Entries { get; set; } = new();

        public UnstructuredObject(StructBin sbin, int id, Object parent, Stream source)
            : base(sbin, id, parent)
        {
            int count = source.ReadSigned16Little();
            Size = source.ReadSigned16Little();
            
            for (int i = 0; i < count; i++)
            {
                var nameRef = source.ReadUnsigned16Little();
                var typeId = (FieldType)source.ReadUnsigned16Little();
                var innerOffset = source.ReadSigned32Little();
                if (source.Position != innerOffset)
                {
                    Debug.WriteLine($"UnstructuredObjectEntry InnerOffset ({innerOffset}) != stream position ({source.Position})!");
                    //throw new Exception("UnstructuredObjectEntry InnerOffset != stream position!");
                    source.Seek(innerOffset, SeekOrigin.Begin);
                }
                var value = Value.NewForType(StructBin, typeId)?.ReadFromStream(source, this, null, null, null);

                //Debug.WriteLine($"Name: {sbin.GetString(nameRef)}, type: {Enum.GetName(typeof(FieldType), typeId)} ({typeId:X}), offset = 0x{innerOffset:X4}");

                var entry = new UnstructuredObjectEntry()
                {
                    Name = sbin.StringDatas[nameRef],
                    Type = typeId,
                    InnerOffset = innerOffset,
                    Value = value
                };
                Entries.Add(entry);
            }

        }
    }

    public class ArrayObject : Object
    {
        public FieldType ItemType { get; set; }
        public short ChildKind { get; set; }
        public List<Value> Items { get; set; } = new();

        public ArrayObject(StructBin sbin, int id, Object parent, Stream source)
            : base(sbin, id, parent)
        {
            ItemType = (FieldType)source.ReadSigned16Little();
            ChildKind = source.ReadSigned16Little();
            int count = source.ReadSigned32Little();
            for (int i = 0; i < count; i++)
            {
                var item = Value.NewForType(StructBin, ItemType);
                if (item != null)
                {
                    item = item.ReadFromStream(source, this, null, (sbin.StructDatas.Count > 0 ? sbin.StructDatas[ChildKind] : null), null);
                    Items.Add(item);
                    continue;
                }
                Debug.WriteLine($"Failed to get Value.NewForType for {ItemType:X}");
            }
        }
    }

    public class EnumObject : Object
    {
        public StringData Name { get; set; }
        public Dictionary<int, StringData> Items { get; private set; } = new();

        public EnumObject(EnumData enumData, Stream source)
            : base(enumData.StructBin, (int)enumData.ObjReference, null)
        {
            Common.AssertEquals((int)FieldType.String, source.ReadSigned32Little(), "Expected EnumObject to contain strings");
            int count = source.ReadSigned32Little();
            for (int i = 0; i < count; i++)
            {
                var item = new StringValue() {
                    StructBin = StructBin
                }.ReadFromStream(source, this, null, null, null) as StringValue ?? throw new Exception("WTF");
                Items.Add(i, item.Value);
            }
        }

        public static EnumObject ParseSingular(EnumData enumData)
        {
            var objData = enumData.StructBin.ObjectDatas[(int)enumData.ObjReference];
            Common.AssertEquals(objData.ObjectDefinitionSize / 4, 2, "Expected an array-like object");

            return new EnumObject(enumData, new MemoryStream(objData.AlignedData))
            {
                Name = enumData.StructBin.StringDatas[enumData.NameId]
            };
        }
    }
}
