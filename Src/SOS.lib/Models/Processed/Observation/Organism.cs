namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     A particular organism or defined group of organisms considered to be taxonomically homogeneous.
    /// </summary>
    /// <remarks>
    ///     Instances of the dwc:Organism class are intended to facilitate linking one or more dwc:Identification instances to
    ///     one or more dwc:Occurrence instances.
    ///     Therefore, things that are typically assigned scientific names (such as viruses, hybrids, and lichens) and
    ///     aggregates whose occurrences are typically
    ///     recorded (such as packs, clones, and colonies) are included in the scope of this class.
    /// </remarks>
    /// <example>
    ///     A specific bird. A specific wolf pack. A specific instance of a bacterial culture.
    /// </example>
    public class Organism
    {
        /// <summary>
        ///     An identifier for the Organism instance (as opposed to a particular digital record of the Organism).
        ///     May be a globally unique identifier or an identifier specific to the data set.
        /// </summary>
        public string OrganismId { get; set; }

        /// <summary>
        ///     A textual name or label assigned to an Organism instance.
        /// </summary>
        public string OrganismName { get; set; }

        /// <summary>
        ///     A description of the kind of Organism instance. Can be used to indicate whether
        ///     the Organism instance represents a discrete organism or if it represents
        ///     a particular type of aggregation..
        /// </summary>
        public string OrganismScope { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of identifiers of other Organisms and their
        ///     associations to this Organism.
        /// </summary>
        public string AssociatedOrganisms { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of previous assignments of names to the Organism.
        /// </summary>
        public string PreviousIdentifications { get; set; }

        /// <summary>
        ///     Comments or notes about the Organism instance..
        /// </summary>
        public string OrganismRemarks { get; set; }
    }
}