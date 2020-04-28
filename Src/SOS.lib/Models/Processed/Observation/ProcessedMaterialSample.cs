namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// A physical result of a sampling (or subsampling) event. In biological collections, the material sample is typically collected,
    /// and either preserved or destructively processed.
    /// </summary>
    /// <example>
    /// A whole organism preserved in a collection. A part of an organism isolated for some purpose. A soil sample. A marine microbial sample.
    /// </example>
    public class ProcessedMaterialSample
    {
        /// <summary>
        /// An identifier for the MaterialSample (as opposed to a particular digital record of the material sample).
        /// In the absence of a persistent global unique identifier, construct one from a combination of identifiers in the record
        /// that will most closely make the materialSampleID globally unique.
        /// </summary>
        /// <example>
        /// 06809dc5-f143-459a-be1a-6f03e63fc083
        /// </example>
        public string MaterialSampleId { get; set; }
    }
}
