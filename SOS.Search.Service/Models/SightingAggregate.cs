using System;
using System.Collections.Generic;

namespace SOS.Search.Service.Models
{
    /// <summary>
    /// Sighting object
    /// </summary>
    public class SightingAggregate : Interfaces.IEntity<int>
    {
        /// <summary>
        /// Id of sighting
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id of taxon
        /// </summary>
        public int TaxonId { get; set; }
    }
}
