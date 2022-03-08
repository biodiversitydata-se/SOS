using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.TaxonTree
{
    public static class TaxonTreeNodeExtensions
    {
        public static IEnumerable<TaxonTreeNode<T>> AsDepthFirstNodeIterator<T>(
            this IEnumerable<TaxonTreeNode<T>> treeNodes,
            bool returnSelfs = false,
            bool returnOnlyUniqueNodes = true)
        {
            if (returnOnlyUniqueNodes)
            {
                return treeNodes.AsUniqueDepthFirstNodeIterator(returnSelfs);
            }

            return treeNodes.AsDepthFirstNodeIterator(returnSelfs);
        }

        public static IEnumerable<TaxonTreeNode<T>> AsDepthFirstNodeIterator<T>(
            this TaxonTreeNode<T> treeNode,
            bool returnSelf = false,
            bool returnOnlyUniqueNodes = true)
        {
            if (returnOnlyUniqueNodes)
            {
                return treeNode.AsUniqueDepthFirstNodeIterator(returnSelf);
            }

            return treeNode.AsDepthFirstNodeIterator(returnSelf);
        }

        private static IEnumerable<TaxonTreeNode<T>> AsDepthFirstNodeIterator<T>(
            this IEnumerable<TaxonTreeNode<T>> treeNodes,
            bool returnSelf = false)
        {
            var stack = new Stack<TaxonTreeNode<T>>();
            foreach (var treeNode in treeNodes)
            {
                foreach (var childNode in treeNode.Children)
                {
                    stack.Push(childNode);
                }
            }

            while (stack.Any())
            {
                var currentNode = stack.Pop();
                yield return currentNode;
                foreach (var childNode in currentNode.Children)
                {
                    stack.Push(childNode);
                }
            }

            if (returnSelf)
            {
                foreach (var treeNode in treeNodes)
                {
                    yield return treeNode;
                }
            }
        }

        private static IEnumerable<TaxonTreeNode<T>> AsUniqueDepthFirstNodeIterator<T>(
            this IEnumerable<TaxonTreeNode<T>> treeNodes,
            bool returnSelf = false)
        {
            var stack = new Stack<TaxonTreeNode<T>>();
            var visitedNodes = new HashSet<TaxonTreeNode<T>>();
            foreach (var treeNode in treeNodes)
            {
                foreach (var childNode in treeNode.Children)
                {
                    stack.Push(childNode);
                }
            }

            while (stack.Any())
            {
                var currentNode = stack.Pop();
                if (!visitedNodes.Contains(currentNode))
                {
                    yield return currentNode;
                    visitedNodes.Add(currentNode);
                }

                foreach (var childNode in currentNode.Children)
                {
                    if (!visitedNodes.Contains(childNode))
                    {
                        stack.Push(childNode);
                    }
                }
            }

            if (returnSelf)
            {
                foreach (var treeNode in treeNodes)
                {
                    if (!visitedNodes.Contains(treeNode))
                    {
                        yield return treeNode;
                        visitedNodes.Add(treeNode);
                    }
                }
            }
        }

        private static IEnumerable<TaxonTreeNode<T>> AsDepthFirstNodeIterator<T>(
            this TaxonTreeNode<T> treeNode,
            bool returnSelf = false)
        {
            var stack = new Stack<TaxonTreeNode<T>>();
            foreach (var childNode in treeNode.Children)
            {
                stack.Push(childNode);
            }

            while (stack.Any())
            {
                var currentNode = stack.Pop();
                yield return currentNode;
                foreach (var childNode in currentNode.Children)
                {
                    stack.Push(childNode);
                }
            }

            if (returnSelf) yield return treeNode;
        }

        private static IEnumerable<TaxonTreeNode<T>> AsUniqueDepthFirstNodeIterator<T>(
            this TaxonTreeNode<T> treeNode,
            bool returnSelf = false)
        {
            var stack = new Stack<TaxonTreeNode<T>>();
            var visitedNodes = new HashSet<TaxonTreeNode<T>>();
            foreach (var childNode in treeNode.Children)
            {
                stack.Push(childNode);
            }

            while (stack.Any())
            {
                var currentNode = stack.Pop();
                if (!visitedNodes.Contains(currentNode))
                {
                    yield return currentNode;
                    visitedNodes.Add(currentNode);
                }

                foreach (var childNode in currentNode.Children)
                {
                    stack.Push(childNode);
                }
            }

            if (returnSelf && !visitedNodes.Contains(treeNode))
            {
                yield return treeNode;
            }
        }

        public static IEnumerable<TaxonTreeNode<T>> AsParentsNodeIterator<T>(
            this TaxonTreeNode<T> treeNode,
            bool returnSelf = false)
        {
            if (returnSelf) yield return treeNode;
            var currentNode = treeNode.Parent;
            while (currentNode != null)
            {
                yield return currentNode;
                currentNode = currentNode.Parent;
            }
        }

        public static HashSet<TaxonTreeEdge<T>> GetParentsEdges<T>(
            this TaxonTreeNode<T> treeNode,
            bool includeSecondaryParents = false)
        {
            HashSet<TaxonTreeEdge<T>> edgeSet = new HashSet<TaxonTreeEdge<T>>();
            HashSet<TaxonTreeNode<T>> visitedNodesSet = new HashSet<TaxonTreeNode<T>>();
            Stack<TaxonTreeNode<T>> nodesStack = new Stack<TaxonTreeNode<T>>();
            nodesStack.Push(treeNode);

            while (nodesStack.Any())
            {
                var node = nodesStack.Pop();
                if (node.Parent != null)
                {
                    var mainEdge = new TaxonTreeEdge<T> { Parent = node.Parent, Child = node, IsMainRelation = true };
                    edgeSet.Add(mainEdge);

                    if (!visitedNodesSet.Contains(node.Parent))
                    {
                        nodesStack.Push(node.Parent);
                    }
                }

                if (includeSecondaryParents && node.SecondaryParents != null && node.SecondaryParents.Count > 0)
                {
                    foreach (var secondaryParent in node.SecondaryParents)
                    {
                        var secondaryEdge = new TaxonTreeEdge<T> { Parent = secondaryParent, Child = node, IsMainRelation = false };                        
                        edgeSet.Add(secondaryEdge);
                        if (!visitedNodesSet.Contains(secondaryParent))
                        {
                            nodesStack.Push(secondaryParent);
                        }                        
                    }
                }

                visitedNodesSet.Add(node);
            }

            return edgeSet;
        }

        public static HashSet<TaxonTreeEdge<T>> GetChildEdges<T>(
            this TaxonTreeNode<T> treeNode,
            bool includeSecondaryChildren = false)
        {
            HashSet<TaxonTreeEdge<T>> edgeSet = new HashSet<TaxonTreeEdge<T>>();
            HashSet<TaxonTreeNode<T>> visitedNodesSet = new HashSet<TaxonTreeNode<T>>();
            Stack<TaxonTreeNode<T>> nodesStack = new Stack<TaxonTreeNode<T>>();
            nodesStack.Push(treeNode);

            while (nodesStack.Any())
            {
                var node = nodesStack.Pop();
                if (node.MainChildren == null && node.MainChildren.Count > 0)
                {
                    foreach (var child in node.MainChildren)
                    {
                        var mainEdge = new TaxonTreeEdge<T> { Parent = node, Child = child, IsMainRelation = true };
                        edgeSet.Add(mainEdge);

                        if (!visitedNodesSet.Contains(child))
                        {
                            nodesStack.Push(child);
                        }
                    }                     
                }

                if (includeSecondaryChildren && node.SecondaryChildren != null && node.SecondaryChildren.Count > 0)
                {
                    foreach (var secondaryChild in node.SecondaryChildren)
                    {
                        var secondaryEdge = new TaxonTreeEdge<T> { Parent = node, Child = secondaryChild, IsMainRelation = false };
                        edgeSet.Add(secondaryEdge);
                        if (!visitedNodesSet.Contains(secondaryChild))
                        {
                            nodesStack.Push(secondaryChild);
                        }
                    }
                }

                visitedNodesSet.Add(node);
            }

            return edgeSet;
        }
    }
}