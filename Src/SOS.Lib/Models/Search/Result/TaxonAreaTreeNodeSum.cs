using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result;

public class TaxonAreaTreeNodeSum
{    
    public int TopologicalIndex { get; set; }
    public TaxonTreeNode<IBasicTaxon> TreeNode { get; set; }
    public int ObservationCount { get; set; }
    public int SumObservationCount { get; set; }
    public Dictionary<string, int> ObservationCountByFeatureId { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> SumObservationCountByFeatureId { get; set; } = new Dictionary<string, int>();
    public int AreaCount { get; set; }
    public int SumAreaCount => DependentFeatureIds == null ? 0 : DependentFeatureIds.Count;
    public HashSet<string> DependentFeatureIds { get; set; }
    public HashSet<int> DependentTaxonIds { get; set; }    

    public override bool Equals(object obj)
    {
        return obj is TaxonAreaTreeNodeSum sum &&
               EqualityComparer<TaxonTreeNode<IBasicTaxon>>.Default.Equals(TreeNode, sum.TreeNode);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TreeNode);
    }

    public override string ToString()
    {
        if (TreeNode != null) return $"TaxonId: {TreeNode.TaxonId}, Count: {ObservationCount:N0}, SumCount: {SumObservationCount:N0}";
        return base.ToString();
    }
}