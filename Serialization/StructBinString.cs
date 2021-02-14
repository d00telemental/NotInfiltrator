using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization
{
    public class StructBinString
    {
        public int Id { get; set; } = 0;
        public Int32 Offset { get; set; } = 0;
        public Int32 Length { get; set; } = 0;
        public string Ascii { get; set; } = null;
    }
}
