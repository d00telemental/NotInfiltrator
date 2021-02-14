using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization.StructBin
{
    public class BaseStructBin
    {
        public string AbsoluteFileName { get; private set; } = null;
        public string FileName { get; private set; } = null;

        public string Magic { get; private set; } = null;
        public int Version { get; private set; } = 0;
        public List<Section> Sections { get; private set; } = new List<Section>();

        protected BaseStructBin(Stream stream, string relativePath = null)
        {
            FileName = relativePath;

            Common.AssertEquals((Magic = stream.ReadAscFixed(4)), "SBIN", "Wrong SBIN magic");
            Common.AssertEquals((Version = stream.ReadSigned32Little()), 3, "Wrong SBIN version");

            while (stream.Position < stream.Length)
            {
                //Debug.WriteLine($"Reading {(fname != null ? fname : "an")} section at 0x{stream.Position:x}");
                Sections.Add(Section.Read(stream));
            }
        }

        protected BaseStructBin(GameFilesystem fs, string relativePath)
            : this(fs.LoadToMemory(relativePath), relativePath)
        {
            AbsoluteFileName = fs.GetAbsolutePath(relativePath);
        }

        public Section FindSection(string name)
            => Sections.Where(s => s.Label == name).Single();
    }
}
