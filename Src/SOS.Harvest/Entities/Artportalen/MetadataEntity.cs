namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Represents different metadata items
    /// </summary>
    public class MetadataEntity
    {
        /// <summary>
        ///     Culture
        /// </summary>
        public string CultureCode { get; set; }

        /// <summary>
        ///     Id of item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Item translation
        /// </summary>
        public string Translation { get; set; }
    }
}