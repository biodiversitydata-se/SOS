using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.TaxonTree
{
    /// <summary>
    /// Taxon tree node.
    /// </summary>    
    public class TaxonTreeNode<T>
    {
        private const int BiotaTaxonId = 0;
        public int TaxonId { get; }
        public string ScientificName { get; }
        public T Data { get; }
        public TaxonTreeNode<T> Parent { get; set; }
        public List<TaxonTreeNode<T>> MainChildren { get; set; }
        public List<TaxonTreeNode<T>> Children { get; set; }
        public List<TaxonTreeNode<T>> SecondaryChildren { get; set; }
        public List<TaxonTreeNode<T>> SecondaryParents { get; set; }

        public TaxonTreeNode(int taxonId) 
            : this(taxonId, null)
        {
        } 

        public TaxonTreeNode(int taxonId, string scientificName)
        {
            TaxonId = taxonId;
            ScientificName = scientificName;
            MainChildren = new List<TaxonTreeNode<T>>();
            Children = new List<TaxonTreeNode<T>>();
            SecondaryChildren = new List<TaxonTreeNode<T>>();
            SecondaryParents = new List<TaxonTreeNode<T>>();
        }

        public TaxonTreeNode(int taxonId, string scientificName, T data)
        {
            TaxonId = taxonId;
            ScientificName = scientificName;
            Data = data;
            MainChildren = new List<TaxonTreeNode<T>>();
            Children = new List<TaxonTreeNode<T>>();
            SecondaryChildren = new List<TaxonTreeNode<T>>();
            SecondaryParents = new List<TaxonTreeNode<T>>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="TaxonTreeNode{T}" />, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="TaxonTreeNode{T}" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="TaxonTreeNode{T}" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        protected bool Equals(TaxonTreeNode<T> other)
        {
            return Equals(TaxonId, other.TaxonId);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((TaxonTreeNode<T>)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return TaxonId.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"TreeNode {TaxonId}";
        }
    }
}