using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

using NotInfiltrator.Serialization;
using NotInfiltrator.Serialization.StructBin;

namespace NotInfiltrator.UI.Converters
{
    public class StructBinToEnums : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<Presentation.EnumData>(
                (value as SemanticStructBin)?.EnumDatas.Select(ed => new Presentation.EnumData(ed, sbin))
                ?? throw new ArgumentException("Passed value should be StructEnums", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
