using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Lib.Models.TaxonTree
{
    /// <summary>
    /// Taxon Tree Factory
    /// </summary>
    public static class TaxonTreeFactory
    {
        private const int BiotaTaxonId = 0;

        /// <summary>
        /// Creates a taxon tree.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static TaxonTree<T> CreateTaxonTree<T>(
            IEnumerable<DarwinCoreTaxon> taxa)
        {
            var treeNodeById = CreateTaxonTreeNodeDictionary<T>(taxa);
            var rootNode = treeNodeById[BiotaTaxonId];
            var tree = new TaxonTree<T>(rootNode, treeNodeById);

            return tree;
        }

        /// <summary>
        /// Creates a taxon tree.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taxonById"></param>
        /// <returns></returns>
        public static TaxonTree<T> CreateTaxonTree<T>(
            Dictionary<int, DarwinCoreTaxon> taxonById)
        {
            var treeNodeById = CreateTaxonTreeNodeDictionary<T>(taxonById);
            var rootNode = treeNodeById[BiotaTaxonId];
            var tree = new TaxonTree<T>(rootNode, treeNodeById);
            return tree;
        }

        /// <summary>
        /// Creates a taxon tree node dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static Dictionary<int, TaxonTreeNode<T>> CreateTaxonTreeNodeDictionary<T>(
            IEnumerable<DarwinCoreTaxon> taxa)
        {
            var taxaById = taxa.ToDictionary(m => m.Id, m => m);
            return CreateTaxonTreeNodeDictionary<T>(taxaById);
        }

        /// <summary>
        /// Creates a taxon tree node dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taxaById"></param>
        /// <returns></returns>
        public static Dictionary<int, TaxonTreeNode<T>> CreateTaxonTreeNodeDictionary<T>(Dictionary<int, DarwinCoreTaxon> taxaById)
        {
            var treeNodeById = new Dictionary<int, TaxonTreeNode<T>>();

            foreach (var taxon in taxaById.Values)
            {
                if (!treeNodeById.TryGetValue(taxon.Id, out var treeNode))
                {
                    treeNode = new TaxonTreeNode<T>(taxon.Id, taxon.ScientificName);
                    treeNodeById.Add(taxon.Id, treeNode);
                }

                // Add main parent
                if (taxon.DynamicProperties.ParentDyntaxaTaxonId.HasValue)
                {
                    if (!treeNodeById.TryGetValue(taxon.DynamicProperties.ParentDyntaxaTaxonId.Value, out var parentNode))
                    {
                        parentNode = new TaxonTreeNode<T>(
                            taxon.DynamicProperties.ParentDyntaxaTaxonId.Value,
                            taxaById[taxon.DynamicProperties.ParentDyntaxaTaxonId.Value].ScientificName);
                        treeNodeById.Add(taxon.DynamicProperties.ParentDyntaxaTaxonId.Value, parentNode);
                    }

                    treeNode.Parent = parentNode;
                    parentNode.MainChildren.Add(treeNode);
                    parentNode.Children.Add(treeNode);
                }

                // Add secondary parents
                if (taxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds != null)
                {
                    foreach (var secondaryParentTaxonId in taxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds)
                    {
                        if (!treeNodeById.TryGetValue(secondaryParentTaxonId, out var secondaryParentNode))
                        {
                            secondaryParentNode = new TaxonTreeNode<T>(
                                secondaryParentTaxonId,
                                taxaById[secondaryParentTaxonId].ScientificName);
                            treeNodeById.Add(secondaryParentTaxonId, secondaryParentNode);
                        }

                        treeNode.SecondaryParents.Add(secondaryParentNode);
                        secondaryParentNode.Children.Add(treeNode);
                        secondaryParentNode.SecondaryChildren.Add(treeNode);
                    }
                }
            }

            return treeNodeById;
        }

        public static TaxonTree<IBasicTaxon> CreateTaxonTree(IEnumerable<IBasicTaxon> taxa)
        {
            var taxonById = taxa.ToDictionary(m => m.Id, m => m);
            var treeNodeById = CreateTaxonTreeNodeDictionary(taxonById);
            var rootNode = treeNodeById[BiotaTaxonId];
            var tree = new TaxonTree<IBasicTaxon>(rootNode, treeNodeById);
            return tree;
        }

        public static Dictionary<int, TaxonTreeNode<IBasicTaxon>> CreateTaxonTreeNodeDictionary(Dictionary<int, IBasicTaxon> taxonById)
        {
            var treeNodeById = new Dictionary<int, TaxonTreeNode<IBasicTaxon>>();

            foreach (var taxon in taxonById.Values)
            {
                if (!treeNodeById.TryGetValue(taxon.Id, out var treeNode))
                {
                    treeNode = new TaxonTreeNode<IBasicTaxon>(taxon.Id, taxon.ScientificName, taxon);
                    treeNodeById.Add(taxon.Id, treeNode);
                }

                // Add main parent
                if (taxon.ParentDyntaxaTaxonId.HasValue)
                {
                    if (!treeNodeById.TryGetValue(taxon.ParentDyntaxaTaxonId.Value, out var parentNode))
                    {
                        parentNode = new TaxonTreeNode<IBasicTaxon>(
                            taxon.ParentDyntaxaTaxonId.Value,
                            taxonById[taxon.ParentDyntaxaTaxonId.Value].ScientificName,
                            taxonById[taxon.ParentDyntaxaTaxonId.Value]);
                        treeNodeById.Add(taxon.ParentDyntaxaTaxonId.Value, parentNode);
                    }

                    treeNode.Parent = parentNode;
                    parentNode.MainChildren.Add(treeNode);
                    parentNode.Children.Add(treeNode);
                }

                // Add secondary parents
                if (taxon.SecondaryParentDyntaxaTaxonIds != null)
                {
                    foreach (var secondaryParentTaxonId in taxon.SecondaryParentDyntaxaTaxonIds)
                    {
                        if (!treeNodeById.TryGetValue(secondaryParentTaxonId, out var secondaryParentNode))
                        {
                            secondaryParentNode = new TaxonTreeNode<IBasicTaxon>(
                                secondaryParentTaxonId,
                                taxonById[secondaryParentTaxonId].ScientificName,
                                taxonById[secondaryParentTaxonId]);
                            treeNodeById.Add(secondaryParentTaxonId, secondaryParentNode);
                        }

                        treeNode.SecondaryParents.Add(secondaryParentNode);
                        secondaryParentNode.Children.Add(treeNode);
                        secondaryParentNode.SecondaryChildren.Add(treeNode);
                    }
                }
            }

            return treeNodeById;
        }
    }
}