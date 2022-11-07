namespace SOS.Lib.Models.Processed.DataStewardship.Dataset
{
    /// <summary>
    /// The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals.
    /// Can contain a range of methods for the same and/or different parts of the data collection.
    /// </summary>
    public class Methodology
    {
        /// <summary>
        /// The title or name of a methodology, e.g. a monitoring manual.
        /// </summary>
        public string MethodologyName { get; set; }

        /// <summary>
        /// Short description of a methodology, e.g. a monitoring manual.
        /// </summary>
        public string MethodologyDescription { get; set; }

        /// <summary>
        /// Persistent link to description of a methodology, e.g. a monitoring manual.
        /// </summary>
        public string MethodologyLink { get; set; }

        /// <summary>
        /// Persistent link to published species list for the dataset.
        /// </summary>
        public string SpeciesList { get; set; }
    }

}