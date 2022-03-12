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
        public static List<TaxonCategory> GetTaxonCategories(TaxonTree<IBasicTaxon> tree)
        {
            var taxonCategoryById = new Dictionary<int, TaxonCategory>();
            foreach (var treeNode in tree.TreeNodeById.Values)
            {
                if (taxonCategoryById.ContainsKey(treeNode.Data.Attributes.TaxonCategoryId)) continue;
                taxonCategoryById.Add(treeNode.Data.Attributes.TaxonCategoryId, CreateTaxonCategory(treeNode));
            }

            foreach (var treeNode in tree.TreeNodeById.Values)
            {
                var taxonCategory = taxonCategoryById[treeNode.Data.Attributes.TaxonCategoryId];
                if (treeNode.Parent != null)
                {
                    taxonCategory.Parents.Add(taxonCategoryById[treeNode.Parent.Data.Attributes.TaxonCategoryId]);
                    taxonCategory.MainParents.Add(taxonCategoryById[treeNode.Parent.Data.Attributes.TaxonCategoryId]);
                }

                if (treeNode.SecondaryParents != null)
                {
                    foreach (var node in treeNode.SecondaryParents)
                    {
                        taxonCategory.Parents.Add(taxonCategoryById[node.Data.Attributes.TaxonCategoryId]);
                        taxonCategory.SecondaryParents.Add(taxonCategoryById[node.Data.Attributes.TaxonCategoryId]);
                    }
                }

                if (treeNode.Children != null)
                {
                    foreach (var node in treeNode.Children)
                    {
                        taxonCategory.Children.Add(taxonCategoryById[node.Data.Attributes.TaxonCategoryId]);
                        taxonCategory.MainChildren.Add(taxonCategoryById[node.Data.Attributes.TaxonCategoryId]);
                    }
                }

                if (treeNode.SecondaryChildren != null)
                {
                    foreach (var node in treeNode.SecondaryChildren)
                    {
                        taxonCategory.Children.Add(taxonCategoryById[node.Data.Attributes.TaxonCategoryId]);
                        taxonCategory.SecondaryChildren.Add(taxonCategoryById[node.Data.Attributes.TaxonCategoryId]);
                    }
                }
            }

            return taxonCategoryById.Values.ToList();
        }

        private static TaxonCategory CreateTaxonCategory(TaxonTreeNode<IBasicTaxon> treeNode)
        {
            var taxonCategory = new TaxonCategory();
            taxonCategory.Id = treeNode.Data.Attributes.TaxonCategoryId;
            taxonCategory.SwedishName = treeNode.Data.Attributes.TaxonCategorySwedish;
            taxonCategory.EnglishName = treeNode.Data.Attributes.TaxonCategoryEnglish;
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

        public static string CreateMermaidDiagram(HashSet<TaxonCategoryEdge> edges)
        {
            var nodes = new HashSet<TaxonCategory>();
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

        public static string CreateGraphVizDiagram(HashSet<TaxonCategoryEdge> edges)
        {
            var nodes = new HashSet<TaxonCategory>();
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