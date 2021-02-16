using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

using NotInfiltrator.Serialization.Data;

namespace NotInfiltrator.UI.Converters
{
    class FieldChildKindTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = value as FieldData ?? throw new NullReferenceException();
            return field.Type switch
            {
                0x10 => field.StructBin.StructDatas[field.ChildKind].Name,  // InlineStruct
                0x11 => field.StructBin.FieldDatas[field.ChildKind] switch  // Array
                {
                    var childKindRef when childKindRef.Type == 0x10 => field.StructBin.StructDatas[childKindRef.ChildKind].Name,
                    var childKindRef when childKindRef.Type == 0x0F => $"{childKindRef.TypeName} (0x{childKindRef.ChildKind:X})",
                    _ => $"? (0x{field.ChildKind:X})"
                },
                0x12 => field.StructBin.EnumDatas[field.ChildKind].Name,    // Enum
                _ => $"0x{field.ChildKind:X}"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
