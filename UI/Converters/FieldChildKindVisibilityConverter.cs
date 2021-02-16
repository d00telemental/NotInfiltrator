using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

using NotInfiltrator.Serialization.Data;

namespace NotInfiltrator.UI.Converters
{
    public class FieldChildKindVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = value as FieldData ?? throw new NullReferenceException();
            return (field?.Type ?? 0) switch
            {
                0x10 => Visibility.Visible,  // InlineStruct
                0x11 => Visibility.Visible,  // Array
                0x12 => Visibility.Visible,  // Enum
                _ => Visibility.Collapsed
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
