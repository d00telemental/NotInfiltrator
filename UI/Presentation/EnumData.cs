using System;
using System.Collections.Generic;
using System.Text;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Presentation
{
    public class EnumData
    {
        private Serialization.StructBin.EnumData _src = null;
        private Serialization.StructBin.SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.GetString(_src.NameStrId);
        public uint ObjReference => _src.ObjReference;

        public EnumData(Serialization.StructBin.EnumData src, Serialization.StructBin.SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }
    }
}
