using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NotInfiltrator.Serialization
{
    [DebuggerDisplay("GameFilesystemNode({Name})")]
    public class GameFilesystemNode
    {
        public string Name { get; private set; } = null;
        public GameFileContent Content { get; set; } = null;
        public List<GameFilesystemNode> Children { get; } = new ();
        public WeakReference<GameFilesystemNode> ParentRef { get; private set; } = null;

        public GameFilesystemNode(GameFilesystemNode parent, string name)
        {
            Name = name;

            if (parent is not null)
            {
                parent.Children.Add(this);
                ParentRef = new (parent);
            }
        }

        public GameFilesystemNode EmplaceChildIfNotExists(string name)
            => FindDirectChild(name) ?? new (this, name);

        public GameFilesystemNode FindDirectChild(string name)
        {
            var children = Children.Where(node => (node.Name?.ToUpper() ?? "") == name.ToUpper());
            return children.Count() == 0 ? null : children.Single();
        }

        public string GetPath(char separator = '\\', bool removeVirtualRoot = true)
        {
            List<string> pathChunks = new ();
            GameFilesystemNode floatNode = this;
            do
            {
                pathChunks.Add(floatNode.Name);
            } while (floatNode.ParentRef?.TryGetTarget(out floatNode) ?? false);

            if (removeVirtualRoot)
            {
                pathChunks = pathChunks.SkipLast(1).ToList();
            }
            pathChunks.Reverse();

            StringBuilder sb = new();
            sb.AppendJoin(separator, pathChunks);
            return sb.ToString();
        }
    }
}
