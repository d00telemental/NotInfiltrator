using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public Object(StructBin sbin, int id, Object parent)
        {
            StructBin = sbin;
            Id = id;
            Parent = parent;
        }
    }

    public record StructuredObjectEntry(int Index, StringData StringData, short Type, int ValueOffset);

    public class StructuredObject : Object
    {
        public List<StructuredObjectEntry> Entries { get; } = new ();
        public int DataSize { get; } = 0;

        public StructuredObject(StructBin sbin, int id, Object parent, ObjectData objectData)
            : base(sbin, id, parent)
        {
            using var dataStream = new MemoryStream(objectData.AlignedData);

            // read the damn thing
        }
    }

    //public class Struct : Object
    //{
    //    public override bool IsArray => false;
    //    public override bool IsStruct => true;
    //    public override bool IsStructuredObject => false;
    //}

    //public class Array : Object
    //{
    //    public override bool IsArray => true;
    //    public override bool IsStruct => false;
    //    public override bool IsStructuredObject => false;
    //}
}
