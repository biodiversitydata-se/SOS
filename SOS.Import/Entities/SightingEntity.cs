using System;

namespace SOS.Import.Entities
{
    /// <summary>
    /// Sighting object
    /// </summary>
    public class SightingEntity
    {
        /// <summary>
        /// Id of activity
        /// </summary>
        public int? ActivityId { get; set; }

        /// <summary>
        /// Sigthing end data
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Sighting end time
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Taxon gender id
        /// </summary>
        public int? GenderId { get; set; }

        /// <summary>
        /// Hidden by provider date
        /// </summary>
        public DateTime? HiddenByProvider { get; set; }

        /// <summary>
        /// Id of sighting
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Taxon length
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Not present flag
        /// </summary>
        public bool NotPresent { get; set; }

        /// <summary>
        /// Not recovered flag
        /// </summary>
        public bool NotRecovered { get; set; }

        /// <summary>
        /// Protected by system flag
        /// </summary>
        public bool ProtectedBySystem { get; set; }

        /// <summary>
        /// Number of taxa found
        /// </summary>
        public int? Quantity { get; set; }
        
        /// <summary>
        /// Id of site
        /// </summary>
        public int? SiteId { get; set; }

        /// <summary>
        /// Taxon stage id
        /// </summary>
        public int? StageId { get; set; }

        /// <summary>
        /// Sif´ghting start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Sighting start time
        /// </summary>
        public TimeSpan? StartTime { get; set; }
        
        /// <summary>
        /// Id of taxon
        /// </summary>
        public int? TaxonId { get; set; }

        /// <summary>
        /// Id of unit
        /// </summary>
        public int? UnitId { get; set; }

        /// <summary>
        /// Un spontaneous flag
        /// </summary>
        public bool Unspontaneous { get; set; }

        /// <summary>
        /// Unsecure determination
        /// </summary>
        public bool UnsureDetermination { get; set; }

        /// <summary>
        /// Taxon weight
        /// </summary>
        public int? Weight { get; set; }
    }
}
