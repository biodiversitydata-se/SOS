namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Represents metadata item with category
    /// </summary>
    public class MetadataWithCategoryEntity<T> : MetadataEntity<T>
    {
        /// <summary>
        ///     Id of category
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        ///     Name of category
        /// </summary>
        public string? CategoryName { get; set; }
    }
}