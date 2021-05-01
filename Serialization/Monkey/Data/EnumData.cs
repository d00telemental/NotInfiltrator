using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Monkey.Data
{
    public class EnumData : Data
    {
        public Int16 NameId { get; init; } = 0;
        public Int32 ObjReference { get; init; } = 0;

        public string Name => StructBin.GetString((ushort)NameId);

        public EnumData(int id, StructBin sbin) : base(id, sbin) { }

        public Monkey.EnumObject Object => StructBin.EnumObjects.First(eo => eo.Name.Text == Name);
    }
}
