namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Represents different metadata items
    /// </summary>
    public class MetadataEntity<T>
    {
        /// <summary>
        /// Constrctor
        /// </summary>
        public MetadataEntity()
        {
            Id = default!;
        }

        /// <summary>
        /// Constrctor
        /// </summary>
        public MetadataEntity(T id)
        {
            Id = id;
        }

        /// <summary>
        ///     Culture
        /// </summary>
        public string? CultureCode { get; set; }

        /// <summary>
        ///     Id of item
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        ///     Item translation
        /// </summary>
        public string? Translation { get; set; }
    }
}