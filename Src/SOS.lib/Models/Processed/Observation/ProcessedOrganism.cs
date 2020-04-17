namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// A particular organism or defined group of organisms considered to be taxonomically homogeneous.
    /// </summary>
    /// <remarks>
    /// Instances of the dwc:Organism class are intended to facilitate linking one or more dwc:Identification instances to one or more dwc:Occurrence instances.
    /// Therefore, things that are typically assigned scientific names (such as viruses, hybrids, and lichens) and aggregates whose occurrences are typically
    /// recorded (such as packs, clones, and colonies) are included in the scope of this class.
    /// </remarks>
    /// <example>
    /// A specific bird. A specific wolf pack. A specific instance of a bacterial culture.
    /// </example>

    public class ProcessedOrganism
    {
        /// <summary>
        /// An identifier for the Organism instance (as opposed to a particular digital record of the Organism).
        /// May be a globally unique identifier or an identifier specific to the data set.
        /// </summary>
        /// <example>
        /// http://arctos.database.museum/guid/WNMU:Mamm:1249
        /// </example>
        public string OrganismId { get; set; }

        /// <summary>
        /// A textual name or label assigned to an Organism instance.
        /// </summary>
        /// <example>
        /// Huberta, Boab Prison Tree, J pod
        /// </example>
        public string OrganismName { get; set; }
    }
}
