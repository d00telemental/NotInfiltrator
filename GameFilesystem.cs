using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace NotInfiltrator
{
    [DebuggerDisplay("GameFilesystemNode({Name})")]
    public class GameFilesystemNode
    {
        public WeakReference<GameFilesystemNode> ParentRef { get; } = null;

        public string Name { get; set; } = null;

        public List<GameFilesystemNode> Children { get; } = new List<GameFilesystemNode>();

        public object Content { get; set; } = null;

        public GameFilesystemNode(GameFilesystemNode parent, string name)
        {
            Name = name;

            if (parent != null)
            {
                parent.Children.Add(this);
                ParentRef = new WeakReference<GameFilesystemNode>(parent);
            }
        }

        public GameFilesystemNode EmplaceChildIfNotExists(string name)
        {
            var existingChild = FindDirectChild(name);
            if (existingChild == null)
            {
                return new GameFilesystemNode(this, name);
            }
            else
            {
                return existingChild;
            }
        }

        public GameFilesystemNode FindDirectChild(string name)
        {
            var children = Children.Where(node => (node.Name?.ToUpper() ?? "") == name.ToUpper());
            return children.Count() == 0 ? null : children.Single();
        }
    }

    public class GameFilesystem
    {
        public string Path = null;

        public Dictionary<string, StructBin> SBinMap = new Dictionary<string, StructBin>();

        public GameFilesystemNode Root = new GameFilesystemNode(null, "Filesystem");

        public GameFilesystem(string rootPath)
        {
            Path = rootPath;
        }

        public string GetRelativePath(string absolutePath)
            => absolutePath.Replace(Path, "");

        public string GetAbsolutePath(string relativePath)
            => Path + relativePath;

        public void LoadAllStructBins()
        {
            var files = Directory.GetFiles(Path, "*.sb", SearchOption.AllDirectories).Select(GetRelativePath).ToArray();

            foreach (var filePath in files)
            {
                var sbin = StructBin.Read(this, filePath);
                SBinMap.Add(filePath, sbin);
                Debug.WriteLine($"Done {sbin.FileName}, {sbin.Entries.Count} entries read.");
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
    }
}
