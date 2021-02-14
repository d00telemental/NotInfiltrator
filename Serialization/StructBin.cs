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
        public string Name { get; private set; } = null;
        public string Magic { get; private set; } = null;
        public int Version { get; private set; } = 0;
        public List<SectionData> Sections { get; private set; } = new List<SectionData>();

        public List<EnumData> EnumDatas { get; private set; } = null;
        public List<StructData> StructDatas { get; private set; } = null;
        public List<FieldData> FieldDatas { get; private set; } = null;
        public List<StringData> Strings { get; private set; } = null;

        public StructBin(GameFilesystem fs, string relativePath)
        {
            Name = relativePath;

            var stream = fs.GetMemoryStreamFor(relativePath);
            Common.AssertEquals(Magic = stream.ReadAscFixed(4), "SBIN", "Wrong SBIN magic");
            Common.AssertEquals(Version = stream.ReadSigned32Little(), 3, "Wrong SBIN version");

            while (stream.Position < stream.Length)
            {
                Sections.Add(new SectionData(Sections.Count, this, stream));
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
            var enumSectionStream = new MemoryStream(GetSection("ENUM").Data);
            while (enumSectionStream.Position < enumSectionStream.Length)
            {
                enums.Add(new EnumData(enums.Count, this, enumSectionStream));
            }
            return enums;
        }

        protected List<StructData> ReadAllStructDatas()
        {
            var structs = new List<StructData>();
            var struSection = GetSection("STRU");
            var struSectionStream = new MemoryStream(struSection.Data);
            while (struSectionStream.Position < struSection.DataLength)  // can't read to stream length here because it = AlignedDataLength
            {
                structs.Add(new StructData(structs.Count, this, struSectionStream));
            }
            return structs;
        }

        protected List<FieldData> ReadAllFieldDatas()
        {
            var fields = new List<FieldData>();
            var fielSectionStream = new MemoryStream(GetSection("FIEL").Data);
            while (fielSectionStream.Position < fielSectionStream.Length)
            {
                fields.Add(new FieldData(fields.Count, this, fielSectionStream));
            }
            return fields;
        }

        protected List<StringData> ReadAllStrings()
        {
            var chdrSection = GetSection("CHDR");
            var cdatSection = GetSection("CDAT");

            var strings = new List<StringData>();
            var chdrSectionStream = new MemoryStream(chdrSection.Data);
            while (chdrSectionStream.Position < chdrSectionStream.Length)
            {
                var offset = chdrSectionStream.ReadSigned32Little();
                var length = chdrSectionStream.ReadSigned32Little();

                var textBuffer = new byte[length];
                Array.Copy(cdatSection.Data, offset, textBuffer, 0, length);

                strings.Add(new StringData
                {
                    Id = strings.Count,
                    Offset = offset,
                    Length = length,
                    Text = Encoding.UTF8.GetString(textBuffer)
                });
            }

            return strings;
        }
        #endregion
    }
}
