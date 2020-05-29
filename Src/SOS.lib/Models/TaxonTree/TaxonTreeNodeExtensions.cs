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
                    stack.Push(childNode);
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
    }
}