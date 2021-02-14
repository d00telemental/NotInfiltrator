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
        public WeakReference<GameFilesystemNode> ParentRef { get; } = null;

        public string Name { get; } = null;

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
}
