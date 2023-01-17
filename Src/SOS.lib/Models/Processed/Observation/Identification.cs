using System;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Identification information about an species observation.
    /// </summary>
    public class Identification
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Identification()
        {

        }

        /// <summary>
        /// Confirmed by.
        /// </summary>
        public string ConfirmedBy { get; set; }

        /// <summary>
        /// Date of confirmation.
        /// </summary>
        public string ConfirmedDate { get; set; }

        /// <summary>
        ///     The date on which the subject was identified as
        ///     representing the Taxon. Recommended best practice is
        ///     to use an encoding scheme, such as ISO 8601:2004(E).
        /// </summary>
        public string DateIdentified { get; set; }

        /// <summary>
        ///     An identifier for the Identification (the body of
        ///     information associated with the assignment of a scientific
        ///     name). May be a global unique identifier or an identifier
        ///     specific to the data set.
        /// </summary>
        public string IdentificationId { get; set; }

        /// <summary>
        ///     A brief phrase or a standard term ("cf.", "aff.") to
        ///     express the determiner's doubts about the Identification.
        /// </summary>
        public string IdentificationQualifier { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of references
        ///     (publication, global unique identifier, URI) used in
        ///     the Identification.
        /// </summary>
        public string IdentificationReferences { get; set; }

        /// <summary>
        ///     Comments or notes about the Identification.
        ///     Contains for example information about that
        ///     the observer is uncertain about which species
        ///     that has been observed.
        /// </summary>
        public string IdentificationRemarks { get; set; }

        /// <summary>
        ///     True if sighting is validated.
        /// This property is deprecated and replaced by the Verified property.
        /// </summary>
        [Obsolete("Replaced by Verified")]
        public bool Validated => Verified;

        /// <summary>
        ///     True if sighting is verified (validated).
        /// </summary>
        public bool Verified { get; set; }

        /// <summary>
        ///     A categorical indicator of the extent to which the taxonomic
        ///     identification has been verified to be correct.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Obsolete("Replaced by VerificationStatus")]
        public VocabularyValue ValidationStatus => VerificationStatus;

        /// <summary>
        ///     A categorical indicator of the extent to which the taxonomic
        ///     identification has been verified to be correct.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        public VocabularyValue VerificationStatus { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of names of people,
        ///     groups, or organizations who assigned the Taxon to the
        ///     subject.
        /// </summary>
        public string IdentifiedBy { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of nomenclatural
        ///     types (type status, typified scientific name, publication)
        ///     applied to the subject.
        /// </summary>
        public string TypeStatus { get; set; }

        /// <summary>
        ///     True if determination is uncertain.
        /// </summary>
        public bool UncertainIdentification { get; set; }

        /// <summary>
        ///    Method used in species determination.
        /// </summary>
        public VocabularyValue DeterminationMethod { get; set; }

        /// <summary>
        ///     A list(concatenated and separated) of names of people,
        ///     who verified the observation.
        /// </summary>
        public string VerifiedBy { get; set; }
    }
}