using CSharpFunctionalExtensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Helpers;

/// <summary>
/// Class for creating taxon relation diagrams.
/// </summary>
public class TaxonRelationDiagramHelper
{
    /// <summary>
    /// Taxon relations tree iteration mode.
    /// </summary>
    public enum TaxonRelationsTreeIterationMode
    {
        /// <summary>
        /// Both parents and children are included.
        /// </summary>
        BothParentsAndChildren,

        /// <summary>
        /// Only parents are included.
        /// </summary>
        OnlyParents,

        /// <summary>
        /// Only children are included.
        /// </summary>
        OnlyChildren
    }

    /// <summary>
    /// Create a taxon relation GraphViz diagram.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="taxonTree"></param>
    /// <param name="taxonIds"></param>
    /// <param name="taxonCategoryById"></param>
    /// <param name="treeIterationMode"></param>
    /// <param name="includeSecondaryRelations"></param>
    /// <returns></returns>
    public static Result<string> CreateGraphvizFormatRepresentation(
        TaxonTree<IBasicTaxon> taxonTree,
        IEnumerable<int> taxonIds,
        Dictionary<int, string> taxonCategoryById,
        TaxonRelationsTreeIterationMode treeIterationMode,
        bool includeSecondaryRelations = true)
    {
        var taxonTreeNodes = taxonTree.GetTreeNodes(taxonIds);
        var edges = GetAllEdges(
            taxonTreeNodes,
            treeIterationMode,
            includeSecondaryRelations);

        const int maxEdges = 100;
        if (edges.Count > maxEdges)
        {
            return Result.Failure<string>($"Too many edges. Your graph contains {edges.Count} edges. Limit is: {maxEdges}");
        }

        string str = CreateGraphvizFormatRepresentation(edges, taxonCategoryById);
        return str;
    }

    /// <summary>
    /// Create a taxon relation Mermaid diagram.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="taxonTree"></param>
    /// <param name="taxonIds"></param>
    /// <param name="taxonCategoryById"></param>
    /// <param name="treeIterationMode"></param>
    /// <param name="includeSecondaryRelations"></param>
    /// <returns></returns>
    public static Result<string> CreateMermaidFormatRepresentation(
        TaxonTree<IBasicTaxon> taxonTree,
        IEnumerable<int> taxonIds,
        Dictionary<int, string> taxonCategoryById,
        TaxonRelationsTreeIterationMode treeIterationMode,
        bool includeSecondaryRelations = true)
    {
        var taxonTreeNodes = taxonTree.GetTreeNodes(taxonIds);
        ICollection<TaxonTreeEdge<IBasicTaxon>> edges = GetAllEdges(
            taxonTreeNodes,
            treeIterationMode,
            includeSecondaryRelations);

        const int maxEdges = 100;
        if (edges.Count > maxEdges)
        {
            return Result.Failure<string>($"Too many edges. Your graph contains {edges.Count} edges. Limit is: {maxEdges}");
        }

        string str = CreateMermaidFormatRepresentation(edges, taxonCategoryById);
        return str;
    }

    private static string CreateGraphvizFormatRepresentation<T>(ICollection<TaxonTreeEdge<T>> edges, Dictionary<int, string> taxonCategoryById)
    {
        HashSet<TaxonTreeNode<T>> nodes = new HashSet<TaxonTreeNode<T>>();
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
            string label = string.Format(
                "{0}\\n{1}",
                node.ScientificName.Replace("/", ""),
                node.TaxonId);
            //string label = string.Format(
            //    "{0}\\n{1}\\n{2}",
            //    node.ScientificName.Replace("/", ""),
            //    node.TaxonId,
            //    node.Category.Name);

            sb.AppendLine(string.Format(
                "node_{0} [label=\"{1}\", shape=box, style=rounded, color=black, peripheries=1, penwidth=1];",
                node.TaxonId,
                label));
        }

        foreach (var edge in edges)
        {
            sb.AppendLine(string.Format(
                "node_{0} -> node_{1} [style={2}, color=black];",
                edge.Parent.TaxonId,
                edge.Child.TaxonId,
                edge.IsMainRelation ? "solid" : "dashed"));
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string CreateMermaidFormatRepresentation(ICollection<TaxonTreeEdge<IBasicTaxon>> edges, Dictionary<int, string> taxonCategoryById)
    {
        HashSet<TaxonTreeNode<IBasicTaxon>> nodes = new HashSet<TaxonTreeNode<IBasicTaxon>>();
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
            string label;
            string taxonCategory;
            if (node.Data.Attributes?.TaxonCategory?.Id != null && taxonCategoryById.TryGetValue(node.Data.Attributes.TaxonCategory.Id, out var categoryName))
            {
                taxonCategory = categoryName;
            }
            else
            {
                taxonCategory = null;
            }

            if (!string.IsNullOrEmpty(taxonCategory))
            {
                label = $"{node.ScientificName.Replace("/", "")}<br/>[{taxonCategory}]<br/>{node.TaxonId}";
            }
            else
            {
                label = $"{node.ScientificName.Replace("/", "")}<br/>{node.TaxonId}";
            }
            
            sb.AppendLine($"    node_{node.TaxonId}(\"{label}\")");
        }

        foreach (var edge in edges)
        {
            sb.AppendLine(string.Format(
                "    node_{0} {2} node_{1}",
                edge.Parent.TaxonId,
                edge.Child.TaxonId,
                edge.IsMainRelation ? "-->" : "-.->"));
        }

        return sb.ToString();
    }

    private static HashSet<TaxonTreeEdge<T>> GetAllEdges<T>(
        ICollection<TaxonTreeNode<T>> treeNodes,
        TaxonRelationsTreeIterationMode treeIterationMode,
        bool includeSecondaryRelations = true)
    {
        var edgesSet = new HashSet<TaxonTreeEdge<T>>();

        if (treeIterationMode == TaxonRelationsTreeIterationMode.BothParentsAndChildren ||
            treeIterationMode == TaxonRelationsTreeIterationMode.OnlyParents)
        {
            edgesSet = GetAllParentEdges(treeNodes, includeSecondaryRelations);
        }

        if (treeIterationMode == TaxonRelationsTreeIterationMode.BothParentsAndChildren ||
            treeIterationMode == TaxonRelationsTreeIterationMode.OnlyChildren)
        {
            var childrenEdges = GetAllChildrenEdges(treeNodes, includeSecondaryRelations);
            edgesSet.UnionWith(childrenEdges);
        }

        return edgesSet;
    }

    /// <summary>
    /// Gets all parent edges hierarchical.
    /// </summary>
    /// <param name="treeNodes">The tree nodes.</param>
    /// <param name="includeSecondaryRelations"></param>        
    /// <returns>Parent edges hierarchical</returns>
    private static HashSet<TaxonTreeEdge<T>> GetAllParentEdges<T>(
        ICollection<TaxonTreeNode<T>> treeNodes,
        bool includeSecondaryRelations = true)
    {
        var edgeSet = new HashSet<TaxonTreeEdge<T>>();
        foreach (var node in treeNodes)
        {
            HashSet<TaxonTreeEdge<T>> parentEdges = node.GetParentsEdges(includeSecondaryRelations);
            edgeSet.UnionWith(parentEdges);
        }

        return edgeSet;
    }

    private static HashSet<TaxonTreeEdge<T>> GetAllChildrenEdges<T>(
        ICollection<TaxonTreeNode<T>> treeNodes,
        bool includeSecondaryRelations = true)
    {
        var edgeSet = new HashSet<TaxonTreeEdge<T>>();
        foreach (var node in treeNodes)
        {
            HashSet<TaxonTreeEdge<T>> childEdges = node.GetChildEdges(includeSecondaryRelations);
            edgeSet.UnionWith(childEdges);
        }

        return edgeSet;
    }
}
