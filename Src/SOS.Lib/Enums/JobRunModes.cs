namespace SOS.Lib.Enums
{
    /// <summary>
    /// Different types of harvest/processing
    /// </summary>
    public enum JobRunModes
    {
        /// <summary>
        /// Full harvest, processing to inactive instance
        /// </summary>
        Full,

        /// <summary>
        /// Incremental harvest, processing to inactive instance
        /// </summary>
        IncrementalInactiveInstance,

        /// <summary>
        /// Incremental harvest, processing to active instance
        /// </summary>
        IncrementalActiveInstance
    }
}