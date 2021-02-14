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
        public GameFilesystemNode RootNode { get; private set; } = new GameFilesystemNode(null, "Filesystem");
        public Dictionary<string, StructBin> StructBinMap { get; private set; } = new Dictionary<string, StructBin>();


        public GameFilesystem(string rootPath)
        {
            Path = rootPath;
        }

        public void LoadAllStructBins()
        {
            var files = Directory.GetFiles(Path, "*.sb", SearchOption.AllDirectories).Select(GetRelativePath).ToArray();
            foreach (var file in files)
            {
                var sbin = new StructBin(this, file);
                StructBinMap.Add(file, sbin);
                Debug.WriteLine($"Done {sbin.Name}, {sbin.Sections.Count} sections read.");
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

                floatingRoot.Content = sbin;
            }
        }

        public void Load()
        {
            LoadAllStructBins();
            BuildFileTree();
        }

        public MemoryStream GetMemoryStreamFor(string relativePath)
            => new MemoryStream(File.ReadAllBytes(GetAbsolutePath(relativePath)));

        public string GetRelativePath(string absolutePath)
            => absolutePath.Replace(Path, "");

        public string GetAbsolutePath(string relativePath)
            => Path + relativePath;
    }
}
