using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Serialization.Data;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization
{
    public class StructBin
    {
        private MemoryStream ReadingStream = null;

        public string Name { get; private set; } = null;
        public string Magic { get; private set; } = null;
        public int Version { get; private set; } = 0;
        public List<SectionData> Sections { get; private set; } = null;

        public List<EnumData> EnumDatas { get; private set; } = null;
        public List<StructData> StructDatas { get; private set; } = null;
        public List<FieldData> FieldDatas { get; private set; } = null;
        public List<StringData> Strings { get; private set; } = null;

        public StructBin(GameFilesystem fs, string relativePath)
        {
            Name = relativePath;
            ReadingStream = fs.GetMemoryStreamFor(relativePath);

            Common.AssertEquals(Magic = ReadingStream.ReadAscFixed(4), "SBIN", "Wrong SBIN magic");
            Common.AssertEquals(Version = ReadingStream.ReadSigned32Little(), 3, "Wrong SBIN version");

            Sections = new();
            while (ReadingStream.Position < ReadingStream.Length)
            {
                Sections.Add(new(Sections.Count, this, ReadingStream));
            }

            EnumDatas = ReadAllEnumDatas();
            StructDatas = ReadAllStructDatas();
            FieldDatas = ReadAllFieldDatas();
            Strings = ReadAllStrings();
        }

        #region Utility accessors
        public string GetString(UInt16 id)
            => Strings[id].Text;

        public SectionData GetSection(string name)
            => Sections.Where(s => s.Label == name).Single();
        #endregion

        #region Section reading
        protected List<EnumData> ReadAllEnumDatas()
        {
            var enums = new List<EnumData>();
            var enumSection = GetSection("ENUM");

            ReadingStream.Seek(enumSection.Start + enumSection.HeaderSize, SeekOrigin.Begin);
            while (ReadingStream.Position < enumSection.End)
            {
                enums.Add(new(enums.Count, this, ReadingStream));
            }
            return enums;
        }

        protected List<StructData> ReadAllStructDatas()
        {
            var structs = new List<StructData>();
            var struSection = GetSection("STRU");

            ReadingStream.Seek(struSection.Start + struSection.HeaderSize, SeekOrigin.Begin);
            while (ReadingStream.Position < struSection.End)
            {
                structs.Add(new(structs.Count, this, ReadingStream));
            }
            return structs;
        }

        protected List<FieldData> ReadAllFieldDatas()
        {
            var fields = new List<FieldData>();
            var fielSection = GetSection("FIEL");

            ReadingStream.Seek(fielSection.Start + fielSection.HeaderSize, SeekOrigin.Begin);
            while (ReadingStream.Position < fielSection.End)
            {
                fields.Add(new(fields.Count, this, ReadingStream));
            }
            return fields;
        }

        protected List<StringData> ReadAllStrings()
        {
            var chdrSection = GetSection("CHDR");
            var cdatSection = GetSection("CDAT");

            var strings = new List<StringData>();
            var textBuffer = new byte[4096];

            ReadingStream.Seek(chdrSection.Start + chdrSection.HeaderSize, SeekOrigin.Begin);
            while (ReadingStream.Position < chdrSection.End)
            {
                var offset = ReadingStream.ReadSigned32Little();
                var length = ReadingStream.ReadSigned32Little();

                Array.Copy(cdatSection.Data, offset, textBuffer, 0, length);

                strings.Add(new()
                {
                    Id = strings.Count,
                    Offset = offset,
                    Length = length,
                    Text = Encoding.UTF8.GetString(textBuffer, 0, length)
                });
            }

            return strings;
        }
        #endregion
    }
}
