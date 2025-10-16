using System;
using System.Collections.Generic;

namespace IndexBenchmarks
{
    /// <summary>
    /// AVL Tree-based index implementation
    /// Self-balancing binary search tree for O(log n) operations
    /// </summary>
    public class AvlTreeIndex<TLink> : ILinksIndex<TLink> where TLink : struct, IEquatable<TLink>, IComparable<TLink>
    {
        private class AvlNode
        {
            public TLink Source;
            public TLink Target;
            public TLink LinkAddress;
            public AvlNode Left;
            public AvlNode Right;
            public int Height;

            public AvlNode(TLink source, TLink target, TLink linkAddress)
            {
                Source = source;
                Target = target;
                LinkAddress = linkAddress;
                Height = 1;
            }
        }

        private AvlNode _root;
        private int _count;

        public AvlTreeIndex()
        {
            _root = null;
            _count = 0;
        }

        private int Height(AvlNode node) => node?.Height ?? 0;

        private int GetBalance(AvlNode node)
        {
            return node == null ? 0 : Height(node.Left) - Height(node.Right);
        }

        private void UpdateHeight(AvlNode node)
        {
            if (node != null)
                node.Height = 1 + Math.Max(Height(node.Left), Height(node.Right));
        }

        private AvlNode RotateRight(AvlNode y)
        {
            AvlNode x = y.Left;
            AvlNode T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            UpdateHeight(y);
            UpdateHeight(x);

            return x;
        }

        private AvlNode RotateLeft(AvlNode x)
        {
            AvlNode y = x.Right;
            AvlNode T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            UpdateHeight(x);
            UpdateHeight(y);

            return y;
        }

        private int CompareLinks(TLink s1, TLink t1, TLink s2, TLink t2)
        {
            int sourceCmp = s1.CompareTo(s2);
            if (sourceCmp != 0) return sourceCmp;
            return t1.CompareTo(t2);
        }

        private AvlNode InsertNode(AvlNode node, TLink source, TLink target, TLink linkAddress)
        {
            if (node == null)
                return new AvlNode(source, target, linkAddress);

            int cmp = CompareLinks(source, target, node.Source, node.Target);

            if (cmp < 0)
                node.Left = InsertNode(node.Left, source, target, linkAddress);
            else if (cmp > 0)
                node.Right = InsertNode(node.Right, source, target, linkAddress);
            else
            {
                node.LinkAddress = linkAddress;
                return node;
            }

            UpdateHeight(node);
            int balance = GetBalance(node);

            // Left Left Case
            if (balance > 1 && CompareLinks(source, target, node.Left.Source, node.Left.Target) < 0)
                return RotateRight(node);

            // Right Right Case
            if (balance < -1 && CompareLinks(source, target, node.Right.Source, node.Right.Target) > 0)
                return RotateLeft(node);

            // Left Right Case
            if (balance > 1 && CompareLinks(source, target, node.Left.Source, node.Left.Target) > 0)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // Right Left Case
            if (balance < -1 && CompareLinks(source, target, node.Right.Source, node.Right.Target) < 0)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        public void Add(TLink linkAddress, TLink source, TLink target)
        {
            bool wasNew = SearchNode(_root, source, target) == null;
            _root = InsertNode(_root, source, target, linkAddress);
            if (wasNew) _count++;
        }

        private AvlNode SearchNode(AvlNode node, TLink source, TLink target)
        {
            if (node == null) return null;

            int cmp = CompareLinks(source, target, node.Source, node.Target);

            if (cmp < 0)
                return SearchNode(node.Left, source, target);
            else if (cmp > 0)
                return SearchNode(node.Right, source, target);
            else
                return node;
        }

        public TLink Search(TLink source, TLink target)
        {
            var node = SearchNode(_root, source, target);
            return node != null ? node.LinkAddress : default;
        }

        private AvlNode MinValueNode(AvlNode node)
        {
            var current = node;
            while (current.Left != null)
                current = current.Left;
            return current;
        }

        private AvlNode DeleteNode(AvlNode root, TLink source, TLink target, ref bool deleted)
        {
            if (root == null)
                return root;

            int cmp = CompareLinks(source, target, root.Source, root.Target);

            if (cmp < 0)
                root.Left = DeleteNode(root.Left, source, target, ref deleted);
            else if (cmp > 0)
                root.Right = DeleteNode(root.Right, source, target, ref deleted);
            else
            {
                deleted = true;

                if (root.Left == null || root.Right == null)
                {
                    AvlNode temp = root.Left ?? root.Right;
                    if (temp == null)
                    {
                        return null;
                    }
                    else
                    {
                        return temp;
                    }
                }
                else
                {
                    AvlNode temp = MinValueNode(root.Right);
                    root.Source = temp.Source;
                    root.Target = temp.Target;
                    root.LinkAddress = temp.LinkAddress;
                    root.Right = DeleteNode(root.Right, temp.Source, temp.Target, ref deleted);
                    deleted = true;
                }
            }

            if (root == null)
                return root;

            UpdateHeight(root);
            int balance = GetBalance(root);

            // Left Left Case
            if (balance > 1 && GetBalance(root.Left) >= 0)
                return RotateRight(root);

            // Left Right Case
            if (balance > 1 && GetBalance(root.Left) < 0)
            {
                root.Left = RotateLeft(root.Left);
                return RotateRight(root);
            }

            // Right Right Case
            if (balance < -1 && GetBalance(root.Right) <= 0)
                return RotateLeft(root);

            // Right Left Case
            if (balance < -1 && GetBalance(root.Right) > 0)
            {
                root.Right = RotateRight(root.Right);
                return RotateLeft(root);
            }

            return root;
        }

        public bool Remove(TLink linkAddress, TLink source, TLink target)
        {
            bool deleted = false;
            _root = DeleteNode(_root, source, target, ref deleted);
            if (deleted) _count--;
            return deleted;
        }

        private void CollectBySource(AvlNode node, TLink source, List<(TLink, TLink)> result)
        {
            if (node == null) return;

            int cmp = source.CompareTo(node.Source);

            if (cmp < 0)
                CollectBySource(node.Left, source, result);
            else if (cmp > 0)
                CollectBySource(node.Right, source, result);
            else
            {
                CollectBySource(node.Left, source, result);
                result.Add((node.LinkAddress, node.Target));
                CollectBySource(node.Right, source, result);
            }
        }

        public IEnumerable<(TLink linkAddress, TLink target)> GetBySource(TLink source)
        {
            var result = new List<(TLink, TLink)>();
            CollectBySource(_root, source, result);
            return result;
        }

        private void CollectByTarget(AvlNode node, TLink target, List<(TLink, TLink)> result)
        {
            if (node == null) return;

            CollectByTarget(node.Left, target, result);
            if (node.Target.Equals(target))
                result.Add((node.LinkAddress, node.Source));
            CollectByTarget(node.Right, target, result);
        }

        public IEnumerable<(TLink linkAddress, TLink source)> GetByTarget(TLink target)
        {
            var result = new List<(TLink, TLink)>();
            CollectByTarget(_root, target, result);
            return result;
        }

        public int Count => _count;

        public void Clear()
        {
            _root = null;
            _count = 0;
        }
    }
}
