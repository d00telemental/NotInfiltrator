using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.Data
{
    public class StructData
    {
        public int Id { get; private set; } = 0;
        public StructBin StructBin { get; private set; } = null;

        public UInt16 NameStrId { get; set; } = 0;
        public UInt16 FirstFieldId { get; set; } = 0;
        public UInt16 FieldCount { get; set; } = 0;

        public string Name => StructBin.GetString(NameStrId);
        public string CodeText => ComposeCodeDefinition();

        public StructData(int id, StructBin sbin, Stream source)
        {
            Id = id;
            StructBin = sbin;

            NameStrId = source.ReadUnsigned16Little();
            FirstFieldId = source.ReadUnsigned16Little();
            FieldCount = source.ReadUnsigned16Little();
        }

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
                    stringBuilder.Append($"<{StructBin.GetString(childStructRef.NameStrId)}>");
                }
                else if (field.Type == 0x11)
                {
                    var childFieldRef = StructBin.FieldDatas[field.ChildKind];
                    if (childFieldRef.Type == 0x10)
                    {
                        stringBuilder.Append($"<{StructBin.GetString(StructBin.StructDatas[childFieldRef.ChildKind].NameStrId)}>");
                    }
                    else
                    {
                        stringBuilder.Append($"<0x{field.ChildKind:X}>");
                    }
                }
                else if (field.Type == 0x12)
                {
                    var childFieldRef = StructBin.EnumDatas[field.ChildKind];
                    stringBuilder.Append($"<{StructBin.GetString(childFieldRef.NameStrId)}>");
                }

                // Space
                stringBuilder.Append(' ');

                // Field name
                stringBuilder.Append(StructBin.GetString(field.NameStrId));

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
