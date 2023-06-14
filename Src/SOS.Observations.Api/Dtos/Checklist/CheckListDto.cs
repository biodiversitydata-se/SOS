using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos.Vocabulary;

namespace SOS.Observations.Api.Dtos.Checklist
{
    public class ChecklistDto : IEntity<string>
    {
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
        public string EffortTime { get; set; }

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
        public LocationDto Location { get; set; }

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
        public ProjectDto Project { get; set; }

        /// <summary>
        /// Name of controlling user
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        /// Check list register date 
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Taxon id's
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        /// <summary>
        /// Taxon id's found
        /// </summary>
        public IEnumerable<int> TaxonIdsFound { get; set; }

        /// <summary>
        /// Taxon id's not found
        /// </summary>
        public IEnumerable<int> TaxonIdsNotFound => TaxonIds?.Where(t => !TaxonIdsFound?.Contains(t) ?? true);
    }
}