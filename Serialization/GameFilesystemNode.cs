﻿using System;
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
        public GameFilesystemNode FindChildRecursively(IEnumerable<string> nameChunks)
        {
            if (nameChunks is not null && nameChunks.Count() > 0)
            {
                foreach (var child in Children)
                {
                    if (child.Name.ToUpper() == nameChunks.First().ToUpper())
                    {
                        return (nameChunks.Count() == 1) ? child : child.FindChildRecursively(nameChunks.Skip(1));
                    }
                }
            }
            return null;
        }
        public IEnumerable<GameFilesystemNode> FindChildrenRecursively(string endsWith = null)
        {
            foreach (var childNode in Children)
            {
                if (childNode.Content is not null && (endsWith is null || (endsWith is not null && childNode.Name.EndsWith(endsWith))))
                {
                    yield return childNode;
                }
                
                foreach (var grandChildNode in childNode.FindChildrenRecursively(endsWith))
                {
                    yield return grandChildNode;
                }
            }
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

        // TODO: implement something like https://stackoverflow.com/a/12377822
    }
}
