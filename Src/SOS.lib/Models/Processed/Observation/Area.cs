namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///  Area (region) information.
    /// </summary>
    public class Area
    {
        /// <summary>
        ///     FeatureId for the area.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Name of the area.
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
