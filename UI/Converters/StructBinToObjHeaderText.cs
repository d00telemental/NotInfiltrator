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
        public class ObjectData : Data
        {
            public byte[] EncodedOffset { get; private set; }
            public uint Length { get; set; } = 0;

            public uint Offset { get; private set; } = 0;

            public ObjectData(int id, StructBin sbin, uint offset, byte[] encodedOffset)
                : base(id, sbin)
            {
                Offset = offset;
                EncodedOffset = encodedOffset;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as Serialization.StructBin;
            return sbin is null ? String.Empty : GetObjectText(sbin);
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

        private static string GetObjectText(StructBin sbin)
        {
            // Build a list of temp. object datas

            var objectDatas = new List<ObjectData>();

            var headerStream = sbin.Sections["OHDR"].NewMemoryStream();
            var objSectionData = sbin.Sections["DATA"].Data;

            while (headerStream.Position < headerStream.Length)
            {
                var encodedOffset = headerStream.ReadBytes(4);
                var offset = DecodeOffset(encodedOffset);
                if (objectDatas.Count > 0)
                {
                    objectDatas[^1].Length = offset - objectDatas[^1].Offset;
                }
                objectDatas.Add(new(objectDatas.Count, sbin, offset, encodedOffset));
            }
            objectDatas[^1].Length = (uint)objSectionData.Length - objectDatas[^1].Offset;


            // Build a string presentation

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Read {objectDatas.Count} indices...\n");

            foreach (var obj in objectDatas)
            {
                var range = (int)obj.Offset..(int)(obj.Offset + obj.Length);
                var bytes = objSectionData[range];

                stringBuilder.AppendLine($"Object 0x{obj.Id:X}  @  0x{obj.Offset:X}  (len = 0x{obj.Length:X})   // original offset = {BitConverter.ToString(obj.EncodedOffset)}\n");
                stringBuilder.AppendLine(BitConverter.ToString(bytes).Replace('-', ' ') + "\n\n");
            }

            return stringBuilder.ToString();
        }
    }
}
