using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.TaxonTree
{
    /// <summary>
    /// Taxon Tree Factory
    /// </summary>
    public static class TaxonTreeFactory
    {
        private const int BiotaTaxonId = 0;

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