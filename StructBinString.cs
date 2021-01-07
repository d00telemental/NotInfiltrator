using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NotInfiltrator
{
    public class StructBinString
    {
        public int Id { get; set; } = 0;
        public Int32 Offset { get; set; } = 0;
        public Int32 Length { get; set; } = 0;
        public string Ascii { get; set; } = null;
    }

    public static class StructBinStringExtensions
    {
        public static List<StructBinString> GetAllStrings(this StructBin sbin)
        {
            var chdrSection = sbin.FindSection("CHDR");
            var cdatSection = sbin.FindSection("CDAT");

            var strings = new List<StructBinString>();
            var chdrSectionStream = new MemoryStream(chdrSection.Data);
            while (chdrSectionStream.Position < chdrSectionStream.Length)
            {
                var offset = chdrSectionStream.ReadSigned32Little();
                var length = chdrSectionStream.ReadSigned32Little();

                strings.Add(new StructBinString
                {
                    Id = strings.Count(),
                    Offset = offset,
                    Length = length,
                    Ascii = Encoding.ASCII.GetString(cdatSection.Data.Skip(offset).Take(length).ToArray())
                });
            }

            return strings;
        }
    }
}
