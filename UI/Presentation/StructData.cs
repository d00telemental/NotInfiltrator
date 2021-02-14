using System;
using System.Collections.Generic;
using System.Text;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Presentation
{
    public class StructData
    {
        private Serialization.StructBin.StructData _src = null;
        private Serialization.StructBin.SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.GetString(_src.NameStrId);
        public int FirstFieldId => _src.FirstFieldId;
        public int FieldCount => _src.FieldCount;
        public string ProgramText => GetCLikeDefinition();

        public StructData(Serialization.StructBin.StructData src, Serialization.StructBin.SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }

        public string GetCLikeDefinition()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("{ \n");
            for (var fieldId = _src.FirstFieldId; fieldId < _src.FirstFieldId + _src.FieldCount; fieldId++)
            {
                var field = _sbin.FieldDatas[fieldId];

                // Padding
                stringBuilder.Append($"  ");

                // Type
                stringBuilder.Append(field.TypeName?.ToLower() ?? $"unk_0x{field.Type:X}_t");
                if (field.Type == 0x10)
                {
                    var childStructRef = _sbin.StructDatas[field.ChildKind];
                    stringBuilder.Append($"<{_sbin.GetString(childStructRef.NameStrId)}>");
                }
                else if (field.Type == 0x11)
                {
                    var childFieldRef = _sbin.FieldDatas[field.ChildKind];
                    if (childFieldRef.Type == 0x10)
                    {
                        stringBuilder.Append($"<{_sbin.GetString(_sbin.StructDatas[childFieldRef.ChildKind].NameStrId)}>");
                    }
                    else
                    {
                        stringBuilder.Append($"<0x{field.ChildKind:X}>");
                    }
                }
                else if (field.Type == 0x12)
                {
                    var childFieldRef = _sbin.EnumDatas[field.ChildKind];
                    stringBuilder.Append($"<{_sbin.GetString(childFieldRef.NameStrId)}>");
                }

                // Space
                stringBuilder.Append($" ");

                // Field name
                stringBuilder.Append($"{_sbin.GetString(field.NameStrId)}");

                // Semicolon and a new line
                stringBuilder.Append(";");
                if (_src.FirstFieldId + _src.FieldCount - 1 != fieldId)
                {
                    stringBuilder.Append("\n");
                }
            }

            stringBuilder.Append("\n}");
            return stringBuilder.ToString();
        }

    }
}
