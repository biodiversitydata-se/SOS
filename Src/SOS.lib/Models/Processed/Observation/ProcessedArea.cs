namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Represents a area
    /// </summary>
    public class ProcessedArea
    {
        /// <summary>
        ///     Id of area
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Feature Id of area
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }
    }
}