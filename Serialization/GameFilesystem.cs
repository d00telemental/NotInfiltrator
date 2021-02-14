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
        public string Path = null;

        public Dictionary<string, StructBin.BaseStructBin> SBinMap = new Dictionary<string, StructBin.BaseStructBin>();

        public GameFilesystemNode Root = new GameFilesystemNode(null, "Filesystem");

        public GameFilesystem(string rootPath)
        {
            Path = rootPath;
        }

        public MemoryStream LoadToMemory(string relativePath)
        {
            var bytes = File.ReadAllBytes(GetAbsolutePath(relativePath));
            return new MemoryStream(bytes);
        }

        public string GetRelativePath(string absolutePath) => absolutePath.Replace(Path, "");

        public string GetAbsolutePath(string relativePath) => Path + relativePath;

        public void LoadAllStructBins()
        {
            var files = Directory.GetFiles(Path, "*.sb", SearchOption.AllDirectories).Select(GetRelativePath).ToArray();

            foreach (var filePath in files)
            {
                var sbin = new StructBin.SemanticStructBin(this, filePath);
                SBinMap.Add(filePath, sbin);
                Debug.WriteLine($"Done {sbin.FileName}, {sbin.Sections.Count} entries read.");
            }
        }

        public void BuildFileTree()
        {
            foreach (var sbin in SBinMap.Values)
            {
                var path = sbin.FileName;
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var pathChunks = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
                var floatingRoot = Root;
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
    }
}
