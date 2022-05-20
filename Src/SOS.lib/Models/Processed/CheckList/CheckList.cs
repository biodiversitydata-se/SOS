using System;
using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Models.Processed.Checklist
{
    public class Checklist : IEntity<string>
    {
        /// <summary>
        /// Values used internal in Artportalen
        /// </summary>
        public ApInternal ArtportalenInternal { get; set; }

        /// <summary>
        /// Id of data provider
        /// </summary>
        public int DataProviderId { get; set; }

        /// <summary>
        /// Check list edit date 
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Check list end date 
        /// </summary>
        public Event Event { get; set; }

        /// <summary>
        /// Id of checklist
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Name of checklist
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Occurence id's
        /// </summary>
        public IEnumerable<string> OccurrenceIds { get; set; }

        /// <summary>
        /// Project id
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// Name of controlling user
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        /// Check list register date 
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// If effort time is provided, we store it here and use it if we can't calulate it 
        /// </summary>
        public string SamplingEffortTime { get; set; }

        /// <summary>
        /// Taxon id's
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        /// <summary>
        /// Taxon id's found
        /// </summary>
        public IEnumerable<int> TaxonIdsFound { get; set; }
    }
}