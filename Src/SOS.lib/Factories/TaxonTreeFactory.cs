using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Lib.Factories
{
    /// <summary>
    ///     Taxon Tree Factory
    /// </summary>
    public static class TaxonTreeFactory
    {
        private const int BiotaTaxonId = 0;

        public static TaxonTree<IBasicTaxon> CreateTaxonTree(IEnumerable<IBasicTaxon> taxa)
        {
            if (!taxa?.Any() ?? true)
            {
                return null;
            }

            var taxonById = taxa.ToDictionary(m => m.Id, m => m);
            var treeNodeById = CreateTaxonTreeNodeDictionary(taxonById);
            treeNodeById.TryGetValue(BiotaTaxonId, out var rootNode);
            var tree = new TaxonTree<IBasicTaxon>(rootNode, treeNodeById);
            return tree;
        }

        public static Dictionary<int, TaxonTreeNode<IBasicTaxon>> CreateTaxonTreeNodeDictionary(
            Dictionary<int, IBasicTaxon> taxonById)
        {
            if (!taxonById?.Any() ?? true)
            {
                return null;
            }

            var treeNodeById = new Dictionary<int, TaxonTreeNode<IBasicTaxon>>();

            foreach (var taxon in taxonById.Values)
            {
                if (!treeNodeById.TryGetValue(taxon.Id, out var treeNode))
                {
                    treeNode = new TaxonTreeNode<IBasicTaxon>(taxon.Id, taxon.ScientificName, taxon);
                    treeNodeById.Add(taxon.Id, treeNode);
                }

                // Add main parent
                if (taxon.Attributes.ParentDyntaxaTaxonId.HasValue)
                {
                    if (!treeNodeById.TryGetValue(taxon.Attributes.ParentDyntaxaTaxonId.Value, out var parentNode))
                    {
                        if (!taxonById.TryGetValue(taxon.Attributes.ParentDyntaxaTaxonId.Value, out var parentTaxon))
                        {
                            parentTaxon = new BasicTaxon
                            {
                                Id = taxon.Attributes.ParentDyntaxaTaxonId.Value
                            };
                        }

                        parentNode = new TaxonTreeNode<IBasicTaxon>(
                            taxon.Attributes.ParentDyntaxaTaxonId.Value,
                            parentTaxon.ScientificName,
                            parentTaxon);
                        treeNodeById.Add(taxon.Attributes.ParentDyntaxaTaxonId.Value, parentNode);
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
                            if (!taxonById.TryGetValue(secondaryParentTaxonId, out var secondaryParentTaxon))
                            {
                                secondaryParentTaxon = new BasicTaxon
                                {
                                    Id = secondaryParentTaxonId
                                };
                            }

                            secondaryParentNode = new TaxonTreeNode<IBasicTaxon>(
                                secondaryParentTaxonId,
                                secondaryParentTaxon.ScientificName,
                                secondaryParentTaxon);
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