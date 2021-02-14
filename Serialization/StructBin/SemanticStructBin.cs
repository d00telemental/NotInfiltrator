using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.StructBin
{
    public class SemanticStructBin : BaseStructBin
    {
        public List<EnumData> EnumDatas { get; private set; } = null;
        public List<StructData> StructDatas { get; private set; } = null;
        public List<FieldData> FieldDatas { get; private set; } = null;
        public List<String> Strings { get; private set; } = null;

        public SemanticStructBin(GameFilesystem fs, string relativePath)
            : base(fs, relativePath)
        {
            EnumDatas = ReadAllEnumDatas();
            StructDatas = ReadAllStructDatas();
            FieldDatas = ReadAllFieldDatas();
            Strings = ReadAllStrings();
        }

        public string GetString(UInt16 id) => Strings[id].Text;

        protected List<EnumData> ReadAllEnumDatas()
        {
            var enums = new List<EnumData>();

            var enumSection = FindSection("ENUM");
            var enumSectionStream = new MemoryStream(enumSection.Data);
            while (enumSectionStream.Position < enumSection.DataLength)
            {
                enums.Add(new EnumData(enumSectionStream) { Id = enums.Count });
            }

            return enums;
        }

        protected List<StructData> ReadAllStructDatas()
        {
            var structs = new List<StructData>();

            var struSection = FindSection("STRU");
            var struSectionStream = new MemoryStream(struSection.Data);
            while (struSectionStream.Position < struSection.DataLength)
            {
                structs.Add(new StructData(structs.Count, this, struSectionStream));
            }

            return structs;
        }

        protected List<FieldData> ReadAllFieldDatas()
        {
            var fields = new List<FieldData>();
            var fielSectionStream = new MemoryStream(FindSection("FIEL").Data);
            while (fielSectionStream.Position < fielSectionStream.Length)
            {
                fields.Add(new FieldData(fielSectionStream) { Id = fields.Count } );
            }

            return fields;
        }

        protected List<String> ReadAllStrings()
        {
            var chdrSection = FindSection("CHDR");
            var cdatSection = FindSection("CDAT");

            var strings = new List<String>();
            var chdrSectionStream = new MemoryStream(chdrSection.Data);
            while (chdrSectionStream.Position < chdrSectionStream.Length)
            {
                var offset = chdrSectionStream.ReadSigned32Little();
                var length = chdrSectionStream.ReadSigned32Little();

                strings.Add(new String
                {
                    Id = strings.Count,
                    Offset = offset,
                    Length = length,
                    Text = Encoding.UTF8.GetString(cdatSection.Data.Skip(offset).Take(length).ToArray())
                });
            }

            return strings;
        }
    }
}
