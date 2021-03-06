using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Serialization.Monkey;
using NotInfiltrator.Serialization.Monkey.Data;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization
{
    public class StructBin : GameFileContent
    {
        #region File contents as seen in their serialized form
        public string Magic { get; private set; } = null;
        public int Version { get; private set; } = 0;
        public Dictionary<string, SectionData> Sections { get; private set; } = null;
        #endregion

        #region Parsed file datas
        public List<EnumData> EnumDatas { get; private set; } = null;
        public List<StructData> StructDatas { get; private set; } = null;
        public List<FieldData> FieldDatas { get; private set; } = null;
        public List<ObjectData> ObjectDatas { get; private set; } = null;
        public List<StringData> StringDatas { get; private set; } = null;
        #endregion

        public List<Monkey.EnumObject> EnumObjects { get; private set; } = null;
        public Monkey.Object RootObject { get; private set; } = null;

        public string AwfulObjectTextDump => ComposeAwfulObjectTextDump();


        public StructBin(GameFilesystem fs, string relativePath)
        {
            FS = fs;
            Name = relativePath;
        }


        public override void Initialize()
        {
            if (Initialized)
            {
                throw new Exception("StructBin already initialized!");
            }

            ReadingStream = FS.GetMemoryStreamFor(Name);

            Common.AssertEquals(Magic = ReadingStream.ReadAscFixed(4), "SBIN", "Wrong SBIN magic");
            Common.AssertEquals(Version = ReadingStream.ReadSigned32Little(), 3, "Wrong SBIN version");

            // Sections must be read before everything else.
            // Unlike further ReadAll*() functions, this returns
            // a dictionary of "section label => section".
            Sections = ReadSectionPartitioning();

            EnumDatas = ReadAllEnumDatas();
            StructDatas = ReadAllStructDatas();
            FieldDatas = ReadAllFieldDatas();
            ObjectDatas = ReadAllObjectDatas();
            StringDatas = ReadAllStringDatas();

            // Now that all 'data's are read,
            // we have enough data to parse objects.
            EnumObjects = ParseEnums();
            RootObject = ParseObjectTree();
        }

        protected Dictionary<string, SectionData> ReadSectionPartitioning()
        {
            var sections = new Dictionary<string, SectionData>();
            while (ReadingStream.Position < ReadingStream.Length)
            {
                var start = ReadingStream.Position;
                var label = Encoding.ASCII.GetString(ReadingStream.ReadBytes(4));
                var dataLength = ReadingStream.ReadSigned32Little();
                var hash = ReadingStream.ReadSigned32Little();
                var data = ReadingStream.ReadBytes(SectionData.GetAlignedDataLength(label, dataLength));
                var end = start + SectionData.HeaderSize + dataLength;
                var alignedEnd = ReadingStream.Position;

                var sectionData = new SectionData(sections.Count, this)
                {
                    Start = start,
                    End = end,
                    AlignedEnd = alignedEnd,

                    Label = label,
                    DataLength = dataLength,
                    Hash = hash,
                    Data = data
                };

                sections.Add(sectionData.Label, sectionData);
            }
            return sections;
        }

        #region Section reading
        protected List<EnumData> ReadAllEnumDatas()
        {
            var enums = new List<EnumData>();
            var enumSection = Sections["ENUM"];

            ReadingStream.Seek(enumSection.DataOffset, SeekOrigin.Begin);
            while (ReadingStream.Position < enumSection.End)
            {
                var nameId = ReadingStream.ReadSigned16Little();
                Common.Assert(0 == ReadingStream.ReadSigned16Little(), "EnumData padding != 0");  // TODO: write a checker for this, then move reading into the initializer
                var objReference = ReadingStream.ReadSigned32Little();

                enums.Add(new(enums.Count, this)
                {
                    NameId = nameId,
                    ObjReference = objReference
                });
            }
            return enums;
        }

        protected List<StructData> ReadAllStructDatas()
        {
            var structs = new List<StructData>();
            var struSection = Sections["STRU"];

            ReadingStream.Seek(struSection.DataOffset, SeekOrigin.Begin);
            while (ReadingStream.Position < struSection.End)
            {
                structs.Add(new(structs.Count, this)
                {
                    NameId = ReadingStream.ReadUnsigned16Little(),
                    FirstFieldId = ReadingStream.ReadUnsigned16Little(),
                    FieldCount = ReadingStream.ReadUnsigned16Little()
                });
            }
            return structs;
        }

        protected List<FieldData> ReadAllFieldDatas()
        {
            var fields = new List<FieldData>();
            var fielSection = Sections["FIEL"];

            ReadingStream.Seek(fielSection.DataOffset, SeekOrigin.Begin);
            while (ReadingStream.Position < fielSection.End)
            {
                fields.Add(new(fields.Count, this)
                {
                    NameId = ReadingStream.ReadUnsigned16Little(),
                    Type = (Monkey.FieldType)ReadingStream.ReadUnsigned16Little(),
                    Offset = ReadingStream.ReadUnsigned16Little(),
                    ChildKind = ReadingStream.ReadUnsigned16Little()
                });
            }
            return fields;
        }

        protected List<ObjectData> ReadAllObjectDatas()
        {
            var objectOffsets = new List<byte[]>();
            var objectDatas = new List<ObjectData>();

            var ohdrSection = Sections["OHDR"];
            var dataSection = Sections["DATA"];

            ReadingStream.Seek(ohdrSection.DataOffset, SeekOrigin.Begin);
            while (ReadingStream.Position < ohdrSection.End)
            {
                objectOffsets.Add(ReadingStream.ReadBytes(4));
            }
            //Debug.WriteLine($"Read OHDR for {Name}: {objectOffsets.Count} indices");
            

            for (int index = 0; index < objectOffsets.Count; index++)
            {
                var decodedOffset = ObjectData.DecodeOffset(objectOffsets[index]);
                var nextDecodedOffset = index switch
                {
                    var value when value + 1 < objectOffsets.Count => ObjectData.DecodeOffset(objectOffsets[index + 1]),
                    _ => dataSection.Data.Length
                };

                objectDatas.Add(new(index, this)
                {
                    Offset = decodedOffset,
                    AlignedLength = nextDecodedOffset - decodedOffset,
                    AlignedData = dataSection.Data[decodedOffset..nextDecodedOffset],
                    EncodedOffset = objectOffsets[index]
                });
            }

            return objectDatas;
        }

        protected List<StringData> ReadAllStringDatas()
        {
            var chdrSection = Sections["CHDR"];
            var cdatSection = Sections["CDAT"];

            var strings = new List<StringData>();
            var textBuffer = new byte[16384];

            ReadingStream.Seek(chdrSection.DataOffset, SeekOrigin.Begin);
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

        #region Object reading
        protected List<EnumObject> ParseEnums()
            => EnumDatas.Select(ed => EnumObject.ParseSingular(ed)).ToList();

        protected Monkey.Object ParseObjectTree()
            => Monkey.Object.ParseTree(this, 0, null);
        #endregion

        #region Utility accessors
        public string GetString(UInt16 id)
            => StringDatas[id].Text;
        #endregion

        private string ComposeAwfulObjectTextDump()
        {
            var stringBuilder = new StringBuilder();

            foreach (var obj in ObjectDatas)
            {
                stringBuilder.AppendLine($"Object 0x{obj.Id:X}  @  0x{obj.Offset:X}  (al. len = 0x{obj.AlignedLength:X})   // ODS = 0x{obj.ObjectDefinitionSize:X}\n");
                stringBuilder.AppendLine(BitConverter.ToString(obj.AlignedData).Replace('-', ' ') + "\n\n");
            }

            return stringBuilder.ToString();
        }
    }
}
