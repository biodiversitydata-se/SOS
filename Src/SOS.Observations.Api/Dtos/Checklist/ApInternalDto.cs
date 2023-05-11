namespace SOS.Observations.Api.Dtos.Checklist
{
    // Artportalen internal values
    public class ApInternalDto
    {
        /// <summary>
        /// Id of checklist
        /// </summary>
        public int ChecklistId { get; set; }

        /// <summary>
        /// Parent taxon id
        /// </summary>
        public int ParentTaxonId { get; set; }

        /// <summary>
        /// Id of controlling user
        /// </summary>
        public int UserId { get; set; }
    }
}
