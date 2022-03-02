using SOS.Lib.Models.TaxonTree;
using StronglyConnectedComponents.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Observations.Api.IntegrationTests.Utils
{
    internal static class TaxonTreeCyclesDetectionUtil
    {
        public static List<StronglyConnectedComponent<int>> CheckForCycles<T>(TaxonTree<T> tree)
        {
            var graph = CreateGraph(tree);
            var cycles = GetCyclesInGraph(graph);
            return cycles;
        }

        private static List<StronglyConnectedComponent<int>> GetCyclesInGraph(List<Vertex<int>> graph)
        {
            var detector = new StronglyConnectedComponentFinder<int>();
            var components = detector.DetectCycle(graph);
            IEnumerable<StronglyConnectedComponent<int>> cycleComponents = components.Where(m => m.IsCycle);
            return cycleComponents.ToList();
        }

        private static List<Vertex<int>> CreateGraph<T>(TaxonTree<T> tree)
        {
            var vertexById = new Dictionary<int, Vertex<int>>();

            foreach (var treeNode in tree.TreeNodeById.Values)
            {
                if (treeNode.Parent != null)
                {
                    if (!vertexById.ContainsKey(treeNode.Parent.TaxonId))
                    {
                        vertexById.Add(treeNode.Parent.TaxonId, new Vertex<int>(treeNode.Parent.TaxonId));
                    }
                    var parentVertex = vertexById[treeNode.Parent.TaxonId];

                    if (!vertexById.ContainsKey(treeNode.TaxonId))
                    {
                        vertexById.Add(treeNode.TaxonId, new Vertex<int>(treeNode.TaxonId));
                    }
                    var childVertex = vertexById[treeNode.TaxonId];

                    parentVertex.Dependencies.Add(childVertex);
                }

                if (treeNode.SecondaryParents != null && treeNode.SecondaryParents.Count > 0)
                {
                    foreach (var parent in treeNode.SecondaryParents)
                    {
                        if (!vertexById.ContainsKey(parent.TaxonId))
                        {
                            vertexById.Add(parent.TaxonId, new Vertex<int>(parent.TaxonId));
                        }
                        var parentVertex = vertexById[parent.TaxonId];

                        if (!vertexById.ContainsKey(treeNode.TaxonId))
                        {
                            vertexById.Add(treeNode.TaxonId, new Vertex<int>(treeNode.TaxonId));
                        }
                        var childVertex = vertexById[treeNode.TaxonId];

                        parentVertex.Dependencies.Add(childVertex);
                    }
                }
            }

            return vertexById.Values.ToList();
        }
    }
}