using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

using NotInfiltrator.Serialization;
using NotInfiltrator.Serialization.Data;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.UI.Converters
{
    public class StructBinToObjHeaderText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as Serialization.StructBin;
            return sbin is null ? String.Empty : GetObjectsText(sbin);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static string GetObjectsText(StructBin sbin)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Read {sbin.ObjectDatas.Count} indices...\n");

            foreach (var obj in sbin.ObjectDatas)
            {
                stringBuilder.AppendLine($"Object 0x{obj.Id:X}  @  0x{obj.Offset:X}  (len = 0x{obj.Length:X})   // original offset = {BitConverter.ToString(obj.EncodedOffset)}\n");
                stringBuilder.AppendLine(BitConverter.ToString(obj.AlignedData).Replace('-', ' ') + "\n\n");
            }

            return stringBuilder.ToString();
        }
    }
}
