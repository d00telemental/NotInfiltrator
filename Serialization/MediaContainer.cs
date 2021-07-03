using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotInfiltrator.Serialization.Nokia;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.Serialization
{
    public class MediaContainer : GameFileContent
    {
        public static int FileIdentifierLength => 12;
        public byte[] FileIdentifier { get; protected set; } = null;
        public List<Section> Sections { get; protected set; } = new ();

        public MediaContainer(GameFilesystem fs, string relativePath)
        {
            FS = fs;
            Name = relativePath;
        }

        public override void Initialize()
        {
            if (Initialized)
            {
                throw new Exception("MediaContainer already initialized!");
            }

            ReadingStream = FS.GetMemoryStreamFor(Name);

            FileIdentifier = ReadFileIdentifier();
            Sections = ReadSections().ToList();
        }

        protected byte[] ReadFileIdentifier()
        {
            return ReadingStream.ReadBytes(FileIdentifierLength);
        }

        protected IEnumerable<Section> ReadSections()
        {
            while (ReadingStream.Position < ReadingStream.Length)
            {
                yield return new Section(ReadingStream);
            }
        }
    }
}
