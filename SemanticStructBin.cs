using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NotInfiltrator
{
    public class SemanticStructBin
        : StructBin
    {
        public List<StructBinStructData> StructDatas { get; private set; } = null;
        public List<StructBinFieldData> FieldDatas { get; private set; } = null;
        public List<StructBinString> Strings { get; private set; } = null;

        public SemanticStructBin(GameFilesystem fs, string relativePath)
            : base(fs, relativePath)
        {
            StructDatas = ReadAllStructDatas();
            FieldDatas = ReadAllFieldDatas();
            Strings = ReadAllStrings();
        }

        protected List<StructBinStructData> ReadAllStructDatas()
        {
            var structs = new List<StructBinStructData>();

            var struSection = FindSection("STRU");
            var struSectionStream = new MemoryStream(struSection.Data);
            while (struSectionStream.Position < struSection.DataLength)
            {
                structs.Add(new StructBinStructData(struSectionStream) { Id = structs.Count() });
            }

            return structs;
        }

        protected List<StructBinFieldData> ReadAllFieldDatas()
        {
            var fields = new List<StructBinFieldData>();
            var fielSectionStream = new MemoryStream(FindSection("FIEL").Data);
            while (fielSectionStream.Position < fielSectionStream.Length)
            {
                fields.Add(new StructBinFieldData(fielSectionStream) { Id = fields.Count() } );
            }

            return fields;
        }

        protected List<StructBinString> ReadAllStrings()
        {
            var chdrSection = FindSection("CHDR");
            var cdatSection = FindSection("CDAT");

            var strings = new List<StructBinString>();
            var chdrSectionStream = new MemoryStream(chdrSection.Data);
            while (chdrSectionStream.Position < chdrSectionStream.Length)
            {
                var offset = chdrSectionStream.ReadSigned32Little();
                var length = chdrSectionStream.ReadSigned32Little();

                strings.Add(new StructBinString
                {
                    Id = strings.Count(),
                    Offset = offset,
                    Length = length,
                    Ascii = Encoding.ASCII.GetString(cdatSection.Data.Skip(offset).Take(length).ToArray())
                });
            }

            return strings;
        }
    }
}
