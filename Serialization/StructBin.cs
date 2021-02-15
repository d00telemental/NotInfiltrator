﻿using System;
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
        /// <summary>
        /// Stream used for reading operations while parsing.
        /// </summary>
        private MemoryStream ReadingStream = null;

        #region File contents as seen in their serialized form
        public string Name { get; private set; } = null;
        public string Magic { get; private set; } = null;
        public int Version { get; private set; } = 0;
        public Dictionary<string, SectionData> Sections { get; private set; } = null;
        #endregion

        #region Parsed file 'data's
        public List<EnumData> EnumDatas { get; private set; } = null;
        public List<StructData> StructDatas { get; private set; } = null;
        public List<FieldData> FieldDatas { get; private set; } = null;
        public List<StringData> Strings { get; private set; } = null;
        #endregion

        public StructBin(GameFilesystem fs, string relativePath)
        {
            Name = relativePath;
            ReadingStream = fs.GetMemoryStreamFor(relativePath);

            Common.AssertEquals(Magic = ReadingStream.ReadAscFixed(4), "SBIN", "Wrong SBIN magic");
            Common.AssertEquals(Version = ReadingStream.ReadSigned32Little(), 3, "Wrong SBIN version");

            Sections = ReadSectionPartitioning();  // sections must be read before everything else

            EnumDatas = ReadAllEnumDatas();
            StructDatas = ReadAllStructDatas();
            FieldDatas = ReadAllFieldDatas();
            Strings = ReadAllStringDatas();
        }

        #region Utility accessors
        public string GetString(UInt16 id)
            => Strings[id].Text;
        #endregion

        #region Section reading
        protected Dictionary<string, SectionData> ReadSectionPartitioning()
        {
            var sections = new Dictionary<string, SectionData>();
            while (ReadingStream.Position < ReadingStream.Length)
            {
                var sectionData = new SectionData(sections.Count, this, ReadingStream);
                sections.Add(sectionData.Label, sectionData);
            }
            return sections;
        }

        protected List<EnumData> ReadAllEnumDatas()
        {
            var enums = new List<EnumData>();
            var enumSection = Sections["ENUM"];

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
            var struSection = Sections["STRU"];

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
            var fielSection = Sections["FIEL"];

            ReadingStream.Seek(fielSection.Start + fielSection.HeaderSize, SeekOrigin.Begin);
            while (ReadingStream.Position < fielSection.End)
            {
                fields.Add(new(fields.Count, this, ReadingStream));
            }
            return fields;
        }

        protected List<StringData> ReadAllStringDatas()
        {
            var chdrSection = Sections["CHDR"];
            var cdatSection = Sections["CDAT"];

            var strings = new List<StringData>();
            var textBuffer = new byte[4096];

            ReadingStream.Seek(chdrSection.Start + chdrSection.HeaderSize, SeekOrigin.Begin);
            while (ReadingStream.Position < chdrSection.End)
            {
                var offset = ReadingStream.ReadSigned32Little();
                var length = ReadingStream.ReadSigned32Little();

                Array.Copy(cdatSection.Data, offset, textBuffer, 0, length);

                strings.Add(new(strings.Count, this)
                {
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
