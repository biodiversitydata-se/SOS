﻿using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.TaxonTree
{
    /// <summary>
    ///     Taxon tree.
    /// </summary>
    public class TaxonTree<T>
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="treeNodeById"></param>
        public TaxonTree(TaxonTreeNode<T> root, Dictionary<int, TaxonTreeNode<T>> treeNodeById)
        {
            Root = root;
            TreeNodeById = treeNodeById;
        }

        /// <summary>
        ///     Root node.
        /// </summary>
        public TaxonTreeNode<T> Root { get; }

        /// <summary>
        ///     Tree nodes grouped by taxon id.
        /// </summary>
        public Dictionary<int, TaxonTreeNode<T>> TreeNodeById { get; set; }

        /// <summary>
        /// Topological sort in reverse order, bottom-up. Key is TaxonId.
        /// </summary>
        public Dictionary<int, int> ReverseTopologicalSortById { get; set; }

        /// <summary>
        /// Get tree nodes for the given taxon ids.
        /// </summary>
        /// <param name="taxonIds">Taxon ids.</param>
        /// <returns></returns>
        public List<TaxonTreeNode<T>> GetTreeNodes(IEnumerable<int> taxonIds)
        {
            var taxonTreeNodes = new List<TaxonTreeNode<T>>();
            foreach (var taxonId in taxonIds)
            {
                if (TreeNodeById.TryGetValue(taxonId, out var treeNode))
                {
                    taxonTreeNodes.Add(treeNode);
                }
            }

            return taxonTreeNodes;
        }

        /// <summary>
        /// Get tree node.
        /// </summary>
        /// <param name="taxonId">Taxon id.</param>
        /// <returns></returns>
        public TaxonTreeNode<T> GetTreeNode(int taxonId)
        {
            if (TreeNodeById.TryGetValue(taxonId, out var treeNode))
            {
                return treeNode;
            }

            return null;
        }

        /// <summary>
        ///     Get underlying taxa.
        /// </summary>
        /// <param name="taxonIds">The taxon ids that we will get underlying taxa for.</param>
        /// <param name="returnSelfs">If true, the specified taxonIds will also be returned.</param>
        /// <returns></returns>
        public IEnumerable<int> GetUnderlyingTaxonIds(IEnumerable<int> taxonIds, bool returnSelfs)
        {
            var underlyingTaxonIds = new HashSet<int>();
            if (!taxonIds?.Any() ?? true)
            {
                return underlyingTaxonIds;
            }
            var treeNodes = taxonIds.Where(t => TreeNodeById.ContainsKey(t)).Select(t => TreeNodeById[t]);

            foreach (var treeNode in treeNodes.AsDepthFirstNodeIterator(returnSelfs))
            {
                underlyingTaxonIds.Add(treeNode.TaxonId);
            }

            // If we don't find any taxa, return input list. Will result in no hits, since taxon id/s is wrong
            // If we don't do this. Taxon id's will not be used in query and all observations matching the other criteria (if any) will be returned
            return underlyingTaxonIds.Any() ? underlyingTaxonIds : taxonIds;
        }

        /// <summary>
        ///     Get underlying taxa.
        /// </summary>
        /// <param name="taxonId">The taxon id that we will get underlying taxa for.</param>
        /// <param name="returnSelfs">If true, the specified taxonId will also be returned.</param>
        /// <returns></returns>
        public IEnumerable<int> GetUnderlyingTaxonIds(int taxonId, bool returnSelfs)
        {
            return GetUnderlyingTaxonIds(new[] { taxonId }, returnSelfs);
        }
    }
}