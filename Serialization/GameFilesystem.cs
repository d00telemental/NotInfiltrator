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
        public Dictionary<string, GameFileContent> FileContentsMap { get; private set; } = new ();

        public GameFilesystem(string rootPath)
        {
            Path = rootPath;

            LoadAllStructBins();
            LoadAllLocalizations();
            LoadAllMediaContainers();
            Debug.WriteLine($"Finished loading GFCs");

            BuildFileTree();
            Debug.WriteLine($"Finished building FST");
        }

        public void LoadAllStructBins()
        {
            var files = Directory.GetFiles(Path, "*.sb", SearchOption.AllDirectories).Select(GetRelativePath).ToList();
            files.ForEach(fileName => { FileContentsMap.Add(fileName, new StructBin(this, fileName)); });
        }
        public void LoadAllLocalizations()
        {
            var files = Directory.GetFiles(Path, "masseffect.bin", SearchOption.AllDirectories).Select(GetRelativePath).ToList();
            files.ForEach(fileName => { FileContentsMap.Add(fileName, new StructBin(this, fileName)); });
        }
        public void LoadAllMediaContainers()
        {
            var files = Directory.GetFiles(Path, "*.m3g", SearchOption.AllDirectories).Select(GetRelativePath).ToList();
            files.ForEach(fileName => { FileContentsMap.Add(fileName, new MediaContainer(this, fileName)); });
        }

        public void BuildFileTree()
        {
            foreach (var gfc in FileContentsMap.Values)
            {
                var path = gfc.Name;
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

                floatingRoot.Content = gfc;  // set the content of the branch tip
            }
        }

        public MemoryStream GetMemoryStreamFor(string relativePath)
            => new (File.ReadAllBytes(GetAbsolutePath(relativePath)));

        public string GetRelativePath(string absolutePath)
            => absolutePath.Replace(Path, "");
        public string GetAbsolutePath(string relativePath)
            => Path + relativePath;

        public GameFilesystemNode FindNode(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            var pathChunks = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            return RootNode.FindChildRecursively(pathChunks);
        }
    }
}
