using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotInfiltrator.Serialization.Data;
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
            Debug.WriteLine($"SBIN: {sbin.Name}, id: {id}, ODS/4: {objData.ObjectDefinitionSize / 4}");
            switch (objData.ObjectDefinitionSize / 4)
            {
                case 0:
                    // Struct
                    //throw new NotImplementedException();
                    return null;
                case 1:
                    // Unstructured object
                    return new UnstructuredObject(sbin, id, parent, objDataStream);
                case 2:
                    // Array
                    return new ArrayObject(sbin, id, parent, objDataStream);
                default:
                    Debug.WriteLine($"Invalid ODS encountered!");
                    throw new Exception("Invalid ODS encountered!");
            }
        }
    }

    //public class StructObject : Object
    //{

    //}

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
                    Debug.WriteLine($"UnstructuredObjectEntry InnerOffset ({innerOffset:X}) != stream position ({source.Position})!");
                    throw new Exception("UnstructuredObjectEntry InnerOffset != stream position!");
                }
                var value = Value.NewForType(StructBin, typeId)?.ReadFromStream(source, this);

                Debug.WriteLine($"Name: {sbin.GetString(nameRef)}, type: {Enum.GetName(typeof(FieldType), typeId)} ({typeId:X}), offset = 0x{innerOffset:X4}");

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
        public List<Value> Items { get; set; } = new();

        public ArrayObject(StructBin sbin, int id, Object parent, Stream source)
            : base(sbin, id, parent)
        {
            ItemType = (FieldType)source.ReadSigned32Little();
            int count = source.ReadSigned32Little();
            for (int i = 0; i < count; i++)
            {
                var item = Value.NewForType(StructBin, ItemType);
                if (item != null)
                {
                    item = item.ReadFromStream(source, this);
                    Items.Add(item);
                    continue;
                }
                Debug.WriteLine($"Failed to get Value.NewForType for {ItemType:X}");
            }
        }
    }
}
