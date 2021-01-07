using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NotInfiltrator
{
    public class StructBin
    {
        public string AbsoluteFileName { get; set; }
        public string FileName { get; set; }

        public string Magic;
        public int Version;
        public List<StructBinSection> Sections = new List<StructBinSection>();

        protected StructBin(Stream stream, string relativePath = null)
        {
            FileName = relativePath;

            Common.AssertEquals((Magic = stream.ReadAscFixed(4)), "SBIN", "Wrong SBIN magic");
            Common.AssertEquals((Version = stream.ReadSigned32Little()), 3, "Wrong SBIN version");

            while (stream.Position < stream.Length)
            {
                //Debug.WriteLine($"Reading {(fname != null ? fname : "an")} section at 0x{stream.Position:x}");
                Sections.Add(StructBinSection.Read(stream));
            }
        }

        protected StructBin(GameFilesystem fs, string relativePath)
            : this(fs.LoadToMemory(relativePath), relativePath)
        {
            AbsoluteFileName = fs.GetAbsolutePath(relativePath);
        }

        public StructBinSection FindSection(string name)
            => Sections.Where(s => s.Label == name).Single();
    }
}
