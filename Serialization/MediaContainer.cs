using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization
{
    public class MediaContainer : GameFileContent
    {
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

            throw new NotImplementedException();
        }
    }
}
