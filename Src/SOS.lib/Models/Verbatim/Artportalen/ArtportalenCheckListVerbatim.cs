using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    /// <summary>
    ///     Sighting object
    /// </summary>
    public class ArtportalenChecklistVerbatim : IEntity<int>
    {
        /// <summary>
        /// Name of controlling user
        /// </summary>
        public string ControllingUser { get; set; }

        /// <summary>
        /// Id of controlling user
        /// </summary>
        public int ControlingUserId { get; set; }

        /// <summary>
        /// Check list edit date 
        /// </summary>
        public DateTime EditDate { get; set; }

        /// <summary>
        /// Check list end date 
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Id of checklist
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of checklist
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Occurence range
        /// </summary>
        public int OccurrenceRange { get; set; }

        /// <summary>
        /// Occurence x coordinate
        /// </summary>
        public int OccurrenceXCoord { get; set; }

        /// <summary>
        /// Occurence y coordinate
        /// </summary>
        public int OccurrenceYCoord { get; set; }

        /// <summary>
        /// Parent taxon id
        /// </summary>
        public int ParentTaxonId { get; set; }

        /// <summary>
        /// Project id
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// Check list register date 
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Occurence id's
        /// </summary>
        public IEnumerable<int> SightingIds { get; set; }

        /// <summary>
        ///     Id of site
        /// </summary>
        public Site Site { get; set; }

        /// <summary>
        /// Check list start date 
        /// </summary>
        public DateTime StartDate { get; set; }

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