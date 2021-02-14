using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Presentation
{
    public class FieldData
    {
        private Serialization.StructBin.FieldData _src = null;
        private Serialization.StructBin.SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.Strings[_src.NameStrId].Text;
        public string Type => _src.TypeName ?? $"unk_0x{_src.Type:X}_t";
        public string SizeDesc => GetSizeDesc(_src.Size);
        public int Offset => _src.Offset;
        public int ChildKind => _src.ChildKind;

        public FieldData(Serialization.StructBin.FieldData src, Serialization.StructBin.SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }

        private string GetSizeDesc(int size)
            => size switch
            {
                var b when new int[] { 2, 4, 8 }.Contains(b) => $"{b} bytes",
                1 => "1 byte",
                _ => $"Unknown: {size}"
            };
    }
}
