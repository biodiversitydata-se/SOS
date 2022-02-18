namespace SOS.Lib.Models.Processed.CheckList
{
    // Artportalen internal values
    public class ApInternal
    {
        /// <summary>
        /// Id of check list
        /// </summary>
        public int CheckListId { get; set; }

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
