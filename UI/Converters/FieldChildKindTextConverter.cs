using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

using NotInfiltrator.Serialization.Data;
using NotInfiltrator.Serialization.Monkey;

namespace NotInfiltrator.UI.Converters
{
    class FieldChildKindTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = value as FieldData ?? throw new NullReferenceException();
            return field.Type switch
            {
                FieldType.InlineStruct => field.StructBin.StructDatas[field.ChildKind].Name,
                FieldType.Array => field.StructBin.FieldDatas[field.ChildKind] switch
                {
                    var childKindRef when childKindRef.Type == FieldType.InlineStruct => field.StructBin.StructDatas[childKindRef.ChildKind].Name,
                    var childKindRef when childKindRef.Type == FieldType.Reference => $"{childKindRef.TypeName} (0x{childKindRef.ChildKind:X})",
                    _ => $"? (0x{field.ChildKind:X})"
                },
                FieldType.Enum => field.StructBin.EnumDatas[field.ChildKind].Name,
                _ => $"0x{field.ChildKind:X}"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
