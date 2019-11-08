namespace SOS.Lib.Models.Verbatim.SpeciesPortal
{
    /// <summary>
    /// Represents metadata item with category
    /// </summary>
    public class MetadataWithCategory
    {
        /// <summary>
        /// Id of item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category of item
        /// </summary>
        public Metadata Category { get; set; }
    }
}
