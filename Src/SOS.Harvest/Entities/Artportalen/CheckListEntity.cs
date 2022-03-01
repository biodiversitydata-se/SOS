using System;

namespace SOS.Harvest.Entities.Artportalen
{
    public class CheckListEntity
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
        /// Id of check list
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of check list
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Occurence x coordinate
        /// </summary>
        public int OccurrenceXCoord { get; set; }

        /// <summary>
        /// Occurence y coordinate
        /// </summary>
        public int OccurrenceYCoord { get; set; }

        /// <summary>
        /// Occurence range
        /// </summary>
        public int OccurrenceRange { get; set; }

        /// <summary>
        /// Parent taxon id
        /// </summary>
        public int ParentTaxonId { get; set; }

        /// <summary>
        /// Project id
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// Check list register date 
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Site id
        /// </summary>
        public int? SiteId { get; set; }

        /// <summary>
        /// Check list start date 
        /// </summary>
        public DateTime StartDate { get; set; }
    }
}
