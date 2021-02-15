using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class StructData : Data
    {
        public UInt16 NameId { get; init; } = 0;
        public UInt16 FirstFieldId { get; init; } = 0;
        public UInt16 FieldCount { get; init; } = 0;

        public string Name => StructBin.GetString(NameId);
        public string CodeText => ComposeCodeDefinition();
        public List<FieldData> Fields => StructBin.FieldDatas.GetRange(FirstFieldId, FieldCount);
        public int Size => Fields.Select(f => f.Size).Sum();

        public StructData(int id, StructBin sbin) : base(id, sbin) { }

        private string ComposeCodeDefinition()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("{ \n");
            for (var fieldId = FirstFieldId; fieldId < FirstFieldId + FieldCount; fieldId++)
            {
                var field = StructBin.FieldDatas[fieldId];

                // Padding
                stringBuilder.Append($"  ");

                // Type
                stringBuilder.Append(field.TypeName?.ToLower() ?? $"unk_0x{field.Type:X}_t");
                if (field.Type == 0x10)
                {
                    var childStructRef = StructBin.StructDatas[field.ChildKind];
                    stringBuilder.Append($"<{StructBin.GetString(childStructRef.NameId)}>");
                }
                else if (field.Type == 0x11)
                {
                    var childFieldRef = StructBin.FieldDatas[field.ChildKind];
                    if (childFieldRef.Type == 0x10)
                    {
                        stringBuilder.Append($"<{StructBin.GetString(StructBin.StructDatas[childFieldRef.ChildKind].NameId)}>");
                    }
                    else
                    {
                        stringBuilder.Append($"<0x{field.ChildKind:X}>");
                    }
                }
                else if (field.Type == 0x12)
                {
                    var childFieldRef = StructBin.EnumDatas[field.ChildKind];
                    stringBuilder.Append($"<{StructBin.GetString(childFieldRef.NameId)}>");
                }

                // Space
                stringBuilder.Append(' ');

                // Field name
                stringBuilder.Append(StructBin.GetString(field.NameId));

                // Semicolon and a new line
                stringBuilder.Append(';');
                if (FirstFieldId + FieldCount - 1 != fieldId)
                {
                    stringBuilder.Append('\n');
                }
            }

            stringBuilder.Append("\n}");
            return stringBuilder.ToString();
        }
    }
}
