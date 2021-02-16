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
                0x12 => field.StructBin.EnumDatas[field.ChildKind].Name,  // Enum
                _ => field.ChildKind.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
