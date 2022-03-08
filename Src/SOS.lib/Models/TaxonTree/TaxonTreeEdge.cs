using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Models.TaxonTree
{
    /// <summary>
    /// Taxon tree edge.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaxonTreeEdge<T>
    {        
        /// <summary>
        /// Parent.
        /// </summary>
        public TaxonTreeNode<T> Parent { get; set; }
        
        /// <summary>
        /// Child.
        /// </summary>
        public TaxonTreeNode<T> Child { get; set; }
        
        /// <summary>
        /// Inidicates whether this relation is a main relation or a secondary relation.
        /// </summary>
        public bool IsMainRelation { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TaxonTreeEdge<T> edge &&
                   EqualityComparer<TaxonTreeNode<T>>.Default.Equals(Parent, edge.Parent) &&
                   EqualityComparer<TaxonTreeNode<T>>.Default.Equals(Child, edge.Child) &&
                   IsMainRelation == edge.IsMainRelation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Parent, Child, IsMainRelation);
        }

        public override string ToString()
        {
            return $"{Parent.TaxonId} -> {Child.TaxonId}, MainRelation={IsMainRelation}";
        }
    }
}