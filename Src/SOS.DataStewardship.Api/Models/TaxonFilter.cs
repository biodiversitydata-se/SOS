using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Taxon filter.
    /// </summary>
    [DataContract]
    public class TaxonFilter
    { 
        /// <summary>
        /// Dyntaxa taxon ids to match.
        /// </summary>

        [DataMember(Name="ids")]
        public List<int?> Ids { get; set; }
    }
}
