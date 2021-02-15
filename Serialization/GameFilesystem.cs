using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace NotInfiltrator.Serialization
{
    public class GameFilesystem
    {
        public string Path { get; private set; } = null;
        public GameFilesystemNode RootNode { get; private set; } = new (null, "Filesystem");
        public Dictionary<string, StructBin> StructBinMap { get; private set; } = new ();


        public GameFilesystem(string rootPath)
        {
            Path = rootPath;

            LoadAllStructBins();
            BuildFileTree();
        }

        public void LoadAllStructBins()
        {
            var files = Directory.GetFiles(Path, "*.sb", SearchOption.AllDirectories).Select(GetRelativePath);
            foreach (var fileName in files)
            {
                StructBinMap.Add(fileName, new (this, fileName));
            }
        }

        public void BuildFileTree()
        {
            foreach (var sbin in StructBinMap.Values)
            {
                var path = sbin.Name;
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var pathChunks = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
                var floatingRoot = RootNode;
                foreach (var pathChunk in pathChunks)
                {
                    floatingRoot = floatingRoot.EmplaceChildIfNotExists(pathChunk);
                }

                floatingRoot.Content = sbin;  // set the content of the branch tip
            }
        }

        public MemoryStream GetMemoryStreamFor(string relativePath)
            => new (File.ReadAllBytes(GetAbsolutePath(relativePath)));

        public string GetRelativePath(string absolutePath)
            => absolutePath.Replace(Path, "");

        public string GetAbsolutePath(string relativePath)
            => Path + relativePath;
    }
}
