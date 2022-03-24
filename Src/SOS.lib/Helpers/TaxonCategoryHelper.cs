using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOS.Lib.Helpers
{
    /// <summary>
    /// Helper class for taxon categories.
    /// </summary>
    public static class TaxonCategoryHelper
    {
        public static List<TaxonCategory> GetTaxonCategories(Dictionary<int, DarwinCoreTaxon> darwinCoreTaxonById)
        {
            var taxonCategoryById = new Dictionary<int, TaxonCategory>();
            foreach (var taxon in darwinCoreTaxonById.Values)
            {
                int taxonCategoryId = taxon.DynamicProperties.TaxonCategoryId.GetValueOrDefault();
                if (taxonCategoryById.ContainsKey(taxonCategoryId)) continue;
                taxonCategoryById.Add(taxonCategoryId, CreateTaxonCategory(taxon));
            }

            foreach (var taxon in darwinCoreTaxonById.Values)
            {
                int taxonCategoryId = taxon.DynamicProperties.TaxonCategoryId.GetValueOrDefault();
                var taxonCategory = taxonCategoryById[taxonCategoryId];
                if (darwinCoreTaxonById.TryGetValue(taxon.DynamicProperties.ParentDyntaxaTaxonId.GetValueOrDefault(-1), out var parentTaxon))
                {                    
                    var parentTaxonCategory = taxonCategoryById[parentTaxon.DynamicProperties.TaxonCategoryId.GetValueOrDefault()];
                    taxonCategory.Parents.Add(parentTaxonCategory);
                    taxonCategory.MainParents.Add(parentTaxonCategory);
                    parentTaxonCategory.Children.Add(taxonCategory);
                    parentTaxonCategory.MainChildren.Add(taxonCategory);
                }

                if (taxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds != null)
                {
                    foreach (var secondaryParentId in taxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds)
                    {
                        if (darwinCoreTaxonById.TryGetValue(secondaryParentId, out var secondaryParentTaxon))
                        {                            
                            var secondaryParentTaxonCategory = taxonCategoryById[secondaryParentTaxon.DynamicProperties.TaxonCategoryId.GetValueOrDefault()];                            
                            taxonCategory.Parents.Add(secondaryParentTaxonCategory);
                            taxonCategory.SecondaryParents.Add(secondaryParentTaxonCategory);
                            secondaryParentTaxonCategory.Children.Add(taxonCategory);
                            secondaryParentTaxonCategory.SecondaryChildren.Add(taxonCategory);
                        }                        
                    }
                }
            }

            return taxonCategoryById.Values.ToList();
        }

        private static TaxonCategory CreateTaxonCategory(DarwinCoreTaxon darwinCoreTaxon)
        {
            var taxonCategory = new TaxonCategory();
            taxonCategory.Id = darwinCoreTaxon.DynamicProperties.TaxonCategoryId.GetValueOrDefault();
            taxonCategory.SwedishName = darwinCoreTaxon.DynamicProperties.TaxonCategorySwedishName;
            taxonCategory.EnglishName = darwinCoreTaxon.DynamicProperties.TaxonCategoryEnglishName;
            taxonCategory.DwcName = darwinCoreTaxon.DynamicProperties.TaxonCategoryDarwinCoreName;
            return taxonCategory;
        }

        public static HashSet<TaxonCategoryEdge> GetTaxonCategoryEdges(List<TaxonCategory> taxonCategories)
        {
            var edges = new HashSet<TaxonCategoryEdge>();
            foreach (var category in taxonCategories)
            {
                foreach (var parentCategory in category.MainParents)
                {
                    var edge = new TaxonCategoryEdge { Parent = parentCategory, Child = category, IsMainRelation = true };
                    edges.Add(edge);
                }
                foreach (var parentCategory in category.SecondaryParents)
                {
                    var edge = new TaxonCategoryEdge { Parent = parentCategory, Child = category, IsMainRelation = false };
                    edges.Add(edge);
                }
            }

            return edges;
        }

        public static string CreateMermaidDiagram(HashSet<TaxonCategoryEdge> edges, bool includeSecondaryRelations)
        {
            var nodes = new HashSet<TaxonCategory>();
            if (!includeSecondaryRelations)
            {
                edges = new HashSet<TaxonCategoryEdge>(edges.Where(m => m.IsMainRelation));
            }

            foreach (var edge in edges)
            {
                nodes.Add(edge.Parent);
                nodes.Add(edge.Child);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("graph TD");

            //-------------------
            // Create main tree
            //-------------------
            foreach (var node in nodes)
            {
                string label = $"{node.SwedishName.Replace("/", "")} [{node.Id}]";
                sb.AppendLine($"    node_{node.Id}(\"{label}\")");
            }

            foreach (var edge in edges)
            {
                sb.AppendLine(string.Format(
                    "    node_{0} {2} node_{1}",
                    edge.Parent.Id,
                    edge.Child.Id,
                    edge.IsMainRelation ? "-->" : "-.->"));
            }

            return sb.ToString();
        }

        public static string CreateGraphVizDiagram(HashSet<TaxonCategoryEdge> edges, bool includeSecondaryRelations)
        {
            var nodes = new HashSet<TaxonCategory>();
            if (!includeSecondaryRelations)
            {
                edges = new HashSet<TaxonCategoryEdge>(edges.Where(m => m.IsMainRelation));
            }

            foreach (var edge in edges)
            {
                nodes.Add(edge.Parent);
                nodes.Add(edge.Child);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("digraph {");

            //-------------------
            // Create main tree
            //-------------------
            foreach (var node in nodes)
            {
                string label = $"{node.SwedishName.Replace("/", "")} [{node.Id}]";

                sb.AppendLine(string.Format(
                    "node_{0} [label=\"{1}\", shape=box, style=rounded, color=black, peripheries=1, penwidth=1];",
                    node.Id,
                    label));
            }

            foreach (var edge in edges)
            {
                sb.AppendLine(string.Format(
                    "node_{0} -> node_{1} [style={2}, color=black];",
                    edge.Parent.Id,
                    edge.Child.Id,
                    edge.IsMainRelation ? "solid" : "dashed"));
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}