namespace SOS.Batch.Import.AP.Models.Aggregates
{
    /// <summary>
    /// Represents diffrent metadata items
    /// </summary>
    public class MetadataAggregate
    {
        /// <summary>
        /// Id of item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of item
        /// </summary>
        public string Name { get; set; }
    }
}
