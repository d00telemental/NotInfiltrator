using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace NotInfiltrator.UI.Converters
{
    public class IntToFieldSizeDescription : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (int)value switch
            {
                var b when new int[] { 2, 4, 8 }.Contains(b) => $"{b} bytes",
                1 => "1 byte",
                _ => $"N/A ({value})"
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
