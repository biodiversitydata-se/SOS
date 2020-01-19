using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Processed.DarwinCore
{
    /// <summary>
    /// Taxon tree relation.
    /// </summary>
    /// <typeparam name="T">The id data type. Usually string or int.</typeparam>
    public class TaxonRelation<T>
    {
        public T ParentTaxonId { get; set; }
        public T ChildTaxonId { get; set; }
        public bool IsMainRelation { get; set; }
    }

}
