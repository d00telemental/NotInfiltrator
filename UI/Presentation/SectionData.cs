using System;
using System.Collections.Generic;
using System.Text;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Presentation
{
    public class SectionData
    {
        private StructBinSection _src = null;
        private SemanticStructBin _sbin = null;

        public string Label => _src.Label;

        public long Start => _src.Start;

        public int Length => _src.DataLength;

        public int AlignedLength => _src.RealDataLength;

        public int Hash => _src.Hash;

        public SectionData(StructBinSection src, SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }
    }
}
