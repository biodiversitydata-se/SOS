using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonTree
{
    /// <summary>
    /// Taxon category edge.
    /// </summary>
    public class TaxonCategoryEdge
    {
        /// <summary>
        /// Parent.
        /// </summary>
        public TaxonCategory Parent { get; set; }

        /// <summary>
        /// Child.
        /// </summary>
        public TaxonCategory Child { get; set; }

        /// <summary>
        /// Inidicates whether this relation is a main relation or a secondary relation.
        /// </summary>
        public bool IsMainRelation { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TaxonCategoryEdge edge &&
                   EqualityComparer<TaxonCategory>.Default.Equals(Parent, edge.Parent) &&
                   EqualityComparer<TaxonCategory>.Default.Equals(Child, edge.Child) &&
                   IsMainRelation == edge.IsMainRelation;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Parent, Child, IsMainRelation);
        }

        public override string ToString()
        {
            return $"{Parent.EnglishName} -> {Child.EnglishName}, MainRelation={IsMainRelation}";
        }
    }
}