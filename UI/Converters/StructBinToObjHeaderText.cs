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
        public class StructBinTempObjectData
        {
            public int Id { get; set; } = 0;
            public byte[] OrigOffset { get; set; }
            public uint Offset { get; set; } = 0;
            public uint Length { get; set; } = 0;

            public StructBinTempObjectData(int id, uint offset, byte[] origOffset)
            {
                Id = id;
                Offset = offset;
                OrigOffset = origOffset;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as Serialization.StructBin;
            if (sbin is null)
            {
                return string.Empty;
            }

            var stream = sbin.GetSection("OHDR").NewMemoryStream();
            var data = sbin.GetSection("DATA").Data;

            var objs = new List<StructBinTempObjectData>();

            var counter = 0;
            while (stream.Position < stream.Length)
            {
                var origOffset = stream.ReadBytes(4);
                var offset = DecodeOffset(origOffset);
                if (counter > 0)
                {
                    objs[counter - 1].Length = offset - objs[counter - 1].Offset;
                }
                objs.Add(new StructBinTempObjectData(counter++, offset, origOffset));
            }
            objs[counter - 1].Length = (uint)data.Length - objs[counter - 1].Offset;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Read {counter} indices...");
            stringBuilder.AppendLine("\n");

            foreach (var obj in objs)
            {
                stringBuilder.AppendLine($"Object 0x{obj.Id:X2}  @  0x{obj.Offset:X2}  (len = 0x{obj.Length:X2})  // orig. def. = {BitConverter.ToString(obj.OrigOffset)}");
                stringBuilder.AppendLine();

                stringBuilder.AppendLine(BitConverter.ToString(data.Skip((int)obj.Offset).Take((int)obj.Length).ToArray()).Replace("-", " "));
                stringBuilder.AppendLine("\n");
            }

            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static uint DecodeOffset(byte[] b)
        {
            if (b is null || b.Length != 4)
            {
                throw new NullReferenceException();
            }

            UInt32 res = 0;
            res |= (UInt32)((UInt32)b[0] >> (Int32)0x3);
            res |= (UInt32)((UInt32)b[1] << (Int32)0x5);
            res |= (UInt32)((UInt32)b[2] << (Int32)0xD);
            res |= (UInt32)((UInt32)b[3] << (Int32)0x15);

            return res;
        }
    }
}
