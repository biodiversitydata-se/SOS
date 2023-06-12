using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
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

        public static TaxonTree<IBasicTaxon> CreateTaxonTree(IDictionary<int, Taxon> taxa)
        {
            if (!taxa?.Any() ?? true)
            {
                return null;
            }

            return CreateTaxonTree(taxa.ToDictionary(t => t.Key, t => (IBasicTaxon) t.Value));
        }

        public static TaxonTree<IBasicTaxon> CreateTaxonTree(IDictionary<int, IBasicTaxon> taxa)
        {
            if (!taxa?.Any() ?? true)
            {
                return null;
            }

            var treeNodeById = CreateTaxonTreeNodeDictionary(taxa);
            treeNodeById.TryGetValue(BiotaTaxonId, out var rootNode);
            var tree = new TaxonTree<IBasicTaxon>(rootNode, treeNodeById);
            RemoveIllegalRelations(tree); // Temporary fix.
            tree.ReverseTopologicalSortById = CreateReverseTopologicalSort(treeNodeById.Values);
            return tree;
        }

        public static Dictionary<int, TaxonTreeNode<IBasicTaxon>> CreateTaxonTreeNodeDictionary(
            IDictionary<int, IBasicTaxon> taxonById)
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
                if (taxon.Attributes?.ParentDyntaxaTaxonId != null)
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

        /// <summary>
        /// Test run depth first search using QuickGraph library.
        /// </summary>
        /// <param name="treeNodes"></param>
        public static void TestQuickGraphDfs(IEnumerable<TaxonTreeNode<IBasicTaxon>> treeNodes)
        {
            var graph = CreateAdjencyGraph(treeNodes);            
            var dfs = new DepthFirstSearchAlgorithm<int, Edge<int>>(graph);            
            var observer = new VertexPredecessorRecorderObserver<int, Edge<int>>();            
            using (observer.Attach(dfs)) // Attach/detach to DFS events
            {
                dfs.Compute(4000107); // DFS for Mammalia
            }

            IDictionary<int, Edge<int>> edges = observer.VerticesPredecessors;
        }

        /// <summary>
        /// Create a reverse topological sort for the taxon tree nodes.
        /// </summary>
        /// <returns>A dictionary with TaxonId as key and reverse topological index as value.</returns>
        private static Dictionary<int, int> CreateReverseTopologicalSort(IEnumerable<TaxonTreeNode<IBasicTaxon>> treeNodes)
        {
            try
            {
                var graph = CreateAdjencyGraph(treeNodes);
                var topoSortByTaxonId = graph
                    .TopologicalSort()
                    .Reverse()
                    .Select((Value, Index) => new { Value, Index })
                    .ToDictionary(m => m.Value, m => m.Index);                

                return topoSortByTaxonId;
            }
            catch (Exception) 
            {
                // Temporary catch exceptions as long as the graph isn't a directed acyclic graph (DAG)
                return new Dictionary<int, int>();
            }
        }

        private static AdjacencyGraph<int, Edge<int>> CreateAdjencyGraph(IEnumerable<TaxonTreeNode<IBasicTaxon>> treeNodes)
        {
            var graph = new AdjacencyGraph<int, Edge<int>>(true);
            foreach (var treeNode in treeNodes)
            {
                graph.AddVertex(treeNode.TaxonId);
            }

            foreach (var treeNode in treeNodes)
            {
                if (treeNode.Parent != null)
                {
                    graph.AddEdge(new Edge<int>(treeNode.Parent.TaxonId, treeNode.TaxonId));
                }

                if (treeNode.SecondaryParents != null && treeNode.SecondaryParents.Count > 0)
                {
                    foreach (var parent in treeNode.SecondaryParents)
                    {
                        graph.AddEdge(new Edge<int>(parent.TaxonId, treeNode.TaxonId));
                    }
                }
            }

            return graph;
        }

        private static void RemoveIllegalRelations<T>(TaxonTree<T> tree)
        {
            // Temporary fix.
            try
            {
                var taxon_2002715 = tree.GetTreeNode(2002715);
                var taxon_222474 = tree.GetTreeNode(222474);
                var taxon_221107 = tree.GetTreeNode(221107);

                if (taxon_2002715 != null && taxon_222474 != null && taxon_221107 != null)
                {
                    taxon_2002715.SecondaryParents.Remove(taxon_222474);
                    taxon_2002715.SecondaryParents.Remove(taxon_221107);
                    taxon_222474.SecondaryChildren.Remove(taxon_2002715);
                    taxon_221107.SecondaryChildren.Remove(taxon_2002715);
                }
            }
            catch
            {

            }
        }
    }
}