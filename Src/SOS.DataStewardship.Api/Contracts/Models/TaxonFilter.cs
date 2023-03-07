using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Taxon filter.
    /// </summary>
    public class TaxonFilter
    {
        /// <summary>
        /// Dyntaxa taxon ids to match.
        /// </summary>
        public List<int> Ids { get; set; }
    }
}
