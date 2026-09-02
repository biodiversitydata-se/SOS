using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Identification information about an species observation.
/// </summary>
public class Identification
{

    /// <summary>
    /// Confirmed by.
    /// </summary>
    public string? ConfirmedBy { get; set; }

    /// <summary>
    /// Date of confirmation.
    /// </summary>
    public string? ConfirmedDate { get; set; }

    /// <summary>
    /// The date on which the subject was identified as
    /// <br/>representing the Taxon. Recommended best practice is
    /// <br/>to use an encoding scheme, such as ISO 8601:2004(E).
    /// </summary>
    public string? DateIdentified { get; set; }

    /// <summary>
    /// An identifier for the Identification (the body of
    /// <br/>information associated with the assignment of a scientific
    /// <br/>name). May be a global unique identifier or an identifier
    /// <br/>specific to the data set.
    /// </summary>
    public string? IdentificationId { get; set; }

    /// <summary>
    /// A brief phrase or a standard term ("cf.", "aff.") to
    /// <br/>express the determiner's doubts about the Identification.
    /// </summary>
    public string? IdentificationQualifier { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of references
    /// <br/>(publication, global unique identifier, URI) used in
    /// <br/>the Identification.
    /// </summary>
    public string? IdentificationReferences { get; set; }

    /// <summary>
    /// Comments or notes about the Identification.
    /// <br/>Contains for example information about that
    /// <br/>the observer is uncertain about which species
    /// <br/>that has been observed.
    /// </summary>
    public string IdentificationRemarks { get; set; }    

    /// <summary>
    /// True if sighting is verified (validated).
    /// </summary>
    public bool Verified { get; set; }


    public VocabularyValue? VerificationStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people,
    /// <br/>groups, or organizations who assigned the Taxon to the
    /// <br/>subject.
    /// </summary>
    public string? IdentifiedBy { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of nomenclatural
    /// <br/>types (type status, typified scientific name, publication)
    /// <br/>applied to the subject.
    /// </summary>
    public string? TypeStatus { get; set; }

    /// <summary>
    /// True if determination is uncertain.
    /// </summary>
    public bool UncertainIdentification { get; set; }

    public VocabularyValue? DeterminationMethod { get; set; }

    /// <summary>
    /// A list(concatenated and separated) of names of people,
    /// <br/>who verified the observation.
    /// </summary>
    public string? VerifiedBy { get; set; }
}


