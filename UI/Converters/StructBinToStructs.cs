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
    public class StructBinToStructs : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<Presentation.StructData>(
                (value as SemanticStructBin)?.StructDatas.Select(sd => new Presentation.StructData(sd, sbin))
                ?? throw new ArgumentException("Passed value should be StructDatas", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
