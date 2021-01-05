using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NotInfiltrator
{
    public class StructBin
    {
        public string AbsoluteFileName { get; set; }
        public string FileName { get; set; }

        public string Magic;
        public int Version;
        public List<StructBinEntry> Entries = new List<StructBinEntry>();

        public static StructBin Read(Stream stream, string fname = null)
        {
            var sbin = new StructBin();

            sbin.FileName = fname;
            
            Common.AssertEquals((sbin.Magic = stream.ReadAscFixed(4)), "SBIN", "Wrong SBIN magic");
            Common.AssertEquals((sbin.Version = stream.ReadSigned32Little()), 3, "Wrong SBIN version");

            while (stream.Position < stream.Length)
            {
                //Debug.WriteLine($"Reading {(fname != null ? fname : "an")} entry at 0x{stream.Position:x}");
                sbin.Entries.Add(StructBinEntry.Read(stream));
            }

            return sbin;
        }

        public static StructBin Read(GameFilesystem fs, string relativePath)
        {
            var absPath = fs.GetAbsolutePath(relativePath);

            var stream = new MemoryStream(File.ReadAllBytes(absPath));
            var sbin = StructBin.Read(stream, relativePath);
            sbin.AbsoluteFileName = absPath;
            return sbin;
        }
    }
}
