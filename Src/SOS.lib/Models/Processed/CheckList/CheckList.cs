using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Models.Processed.CheckList
{
    public class CheckList : IEntity<string>
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
        ///  Time spend to search
        /// </summary>
        public string EffortTime => !string.IsNullOrEmpty(SamplingEffortTime) || Event?.EndDate == null || Event?.StartDate == null
            ? SamplingEffortTime
            : Event.EndDate.Value.Subtract(Event.StartDate.Value).ToString("g");
        
        /// <summary>
        /// Check list end date 
        /// </summary>
        public Event Event { get; set; }

        /// <summary>
        /// Id of check list
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Name of check list
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
        /// If effort time is provided, we store it here and use it if we can't calulate it (se above)
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string SamplingEffortTime { get; set; }

        /// <summary>
        /// Taxon id's
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        /// <summary>
        /// Taxon id's found
        /// </summary>
        public IEnumerable<int> TaxonIdsFound { get; set; }

        // <summary>
        /// Taxon id's not found
        /// </summary>
        public IEnumerable<int> TaxonIdsNotFound => TaxonIds?.Where(t => !TaxonIdsFound?.Contains(t) ?? true);
    }
}