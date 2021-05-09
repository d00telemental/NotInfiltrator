using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization
{
    public abstract class GameFileContent
    {
        protected GameFilesystem FS = null;
        /// <summary>
        /// Stream to read file contents from. <br/>
        /// Usually instantiated from constructor.
        /// </summary>
        protected MemoryStream ReadingStream = null;

        /// <summary>
        /// Associated file name.
        /// </summary>
        public string Name { get; protected set; } = null;

        /// <summary>
        /// Whether or not this file has been actually parsed from content.
        /// </summary>
        public bool Initialized { get; protected set; } = false;

        /// <summary>
        /// Read the file from content.
        /// </summary>
        public abstract void Initialize();
    }
}
