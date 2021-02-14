using System;
using System.Collections.Generic;
using System.Text;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Presentation
{
    public class FieldData
    {
        private Serialization.StructBin.FieldData _src = null;
        private Serialization.StructBin.SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.Strings[_src.NameStrId].Ascii;
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
                1 => "BYTE",
                2 => "WORD",
                4 => "DWORD",
                8 => "QWORD",
                _ => $"?({size})?"
            };
    }
}
