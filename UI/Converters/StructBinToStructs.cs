using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Converters
{
    public class StructDataToPresentation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("Hit StructDataToPresentation: " + value);
            var data = value as Serialization.StructBin.StructData;
            if (data is not null)
            {
                return new Presentation.StructData(data, data.StructBin);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StructBinToStructs : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as Serialization.StructBin.SemanticStructBin;
            return sbin?.StructDatas;

            //return new ObservableCollection<Presentation.StructData>(
            //    (value as Serialization.StructBin.SemanticStructBin)?.StructDatas.Select(sd => new Presentation.StructData(sd, sbin))
            //    ?? throw new ArgumentException("Passed value should be StructDatas", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
