using System.Collections.Generic;
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
        ///     Get underlying taxa.
        /// </summary>
        /// <param name="taxonIds">The taxon ids that we will get underlying taxa for.</param>
        /// <param name="returnSelfs">If true, the specified taxonIds will also be returned.</param>
        /// <returns></returns>
        public IEnumerable<int> GetUnderlyingTaxonIds(IEnumerable<int> taxonIds, bool returnSelfs)
        {
            if (taxonIds == null) return null;
            var treeNodes = GetTreeNodes(taxonIds);
            var underlyingTaxonIds = new List<int>();
            foreach (var treeNode in treeNodes.AsDepthFirstNodeIterator(returnSelfs))
            {
                underlyingTaxonIds.Add(treeNode.TaxonId);
            }

            return underlyingTaxonIds;
        }

        /// <summary>
        ///     Get underlying taxa.
        /// </summary>
        /// <param name="taxonId">The taxon id that we will get underlying taxa for.</param>
        /// <param name="returnSelfs">If true, the specified taxonId will also be returned.</param>
        /// <returns></returns>
        public IEnumerable<int> GetUnderlyingTaxonIds(int taxonId, bool returnSelfs)
        {
            var treeNode = TreeNodeById[taxonId];
            var underlyingTaxonIds = new List<int>();
            foreach (var node in treeNode.AsDepthFirstNodeIterator(returnSelfs))
            {
                underlyingTaxonIds.Add(node.TaxonId);
            }

            return underlyingTaxonIds;
        }

        private IEnumerable<TaxonTreeNode<T>> GetTreeNodes(IEnumerable<int> taxonIds)
        {
            return taxonIds.Select(t => TreeNodeById[t]);
        }
    }
}