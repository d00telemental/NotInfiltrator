using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class StringData : Data
    {
        public Int32 Offset { get; set; } = 0;
        public Int32 Length { get; set; } = 0;
        public string Text { get; set; } = null;

        public StringData(int id, StructBin sbin) : base(id, sbin) { }
    }
}
