using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

using NotInfiltrator.Serialization.Monkey.Data;
using NotInfiltrator.Serialization.Monkey;

namespace NotInfiltrator.UI.Converters
{
    public class FieldChildKindVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = value as FieldData ?? throw new NullReferenceException();
            return (field?.Type ?? 0) switch
            {
                FieldType.InlineStruct => Visibility.Visible,
                FieldType.Array => Visibility.Visible,
                FieldType.Enum => Visibility.Visible,
                _ => Visibility.Collapsed
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
