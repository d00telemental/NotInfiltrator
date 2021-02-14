using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Converters
{
    public class StructBinToFields : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as Serialization.StructBin.SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<Presentation.FieldData>(
                (value as Serialization.StructBin.SemanticStructBin)?.FieldDatas.Select(fd => new Presentation.FieldData(fd, sbin))
                ?? throw new ArgumentException("Passed value should be FieldDatas", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
