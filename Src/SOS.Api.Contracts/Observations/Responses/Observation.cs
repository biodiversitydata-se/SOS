using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Information about a species observation.
/// </summary>
public class Observation
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DiffusionStatus DiffusionStatus { get; set; }

    /// <summary>
    /// Indicates whether the location is generalized.
    /// </summary>
    public bool IsGeneralized { get; set; }

    /// <summary>
    /// Indicates whether there exists a generalized location in other index.
    /// </summary>
    public bool HasGeneralizedObservationInOtherIndex { get; set; }

    public Event? Event { get; set; }

    public DataStewardshipInfo? DataStewardship { get; set; }

    public GeologicalContext? GeologicalContext { get; set; }

    public Identification? Identification { get; set; }

    public Location? Location { get; set; }

    public MaterialSample? MaterialSample { get; set; }

    public Occurrence? Occurrence { get; set; }

    public Organism? Organism { get; set; }

    public Taxon? Taxon { get; set; }

    public VocabularyValue? AccessRights { get; set; }

    public VocabularyValue? BasisOfRecord { get; set; }

    /// <summary>
    /// A bibliographic reference for the resource as a statement
    /// <br/>indicating how this record should be cited (attributed)
    /// <br/>when used.
    /// <br/>Recommended practice is to include sufficient
    /// <br/>bibliographic detail to identify the resource as
    /// <br/>unambiguously as possible.
    /// </summary>
    public string? BibliographicCitation { get; set; }

    /// <summary>
    /// The name, acronym, coden, or initialism identifying the
    /// <br/>collection or data set from which the record was derived.
    /// </summary>
    public string? CollectionCode { get; set; }

    /// <summary>
    /// An identifier for the collection or dataset from which
    /// <br/>the record was derived.
    /// <br/>For physical specimens, the recommended best practice is
    /// <br/>to use the identifier in a collections registry such as
    /// <br/>the Biodiversity Collections Index
    /// <br/>(http://www.biodiversitycollectionsindex.org/).
    /// </summary>
    public string? CollectionId { get; set; }

    /// <summary>
    /// Actions taken to make the shared data less specific or
    /// <br/>complete than in its original form.
    /// <br/>Suggests that alternative data of higher quality
    /// <br/>may be available on request.
    /// </summary>
    public string? DataGeneralizations { get; set; }

    /// <summary>
    /// Data provider id.
    /// </summary>
    public int DataProviderId { get; set; }

    /// <summary>
    /// An identifier for the set of data.
    /// <br/>May be a global unique identifier or an identifier
    /// <br/>specific to a collection or institution.
    /// </summary>
    public string? DatasetId { get; set; }

    /// <summary>
    /// The name identifying the data set
    /// <br/>from which the record was derived.
    /// </summary>
    public string? DatasetName { get; set; }

    /// <summary>
    /// A list of additional measurements, facts, characteristics, or assertions about the record.
    /// <br/>Meant to provide a mechanism for structured content.
    /// <br/>Recommended best practice is to use a key:value encoding schema for a data interchange format such as JSON.
    /// </summary>
    public string? DynamicProperties { get; set; }

    /// <summary>
    /// Unique id.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Additional information that exists, but that has
    /// <br/>not been shared in the given record.
    /// </summary>
    public string? InformationWithheld { get; set; }

    /// <summary>
    /// An identifier for the institution having custody of the object(s) or information referred to in the record.
    /// </summary>
    public string? InstitutionId { get; set; }

    public VocabularyValue? InstitutionCode { get; set; }

    /// <summary>
    /// A language of the resource.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as RFC 4646 [RFC4646].
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// A legal document giving official permission to do something with the resource.
    /// </summary>
    public string? License { get; set; }

    /// <summary>
    /// The most recent date-time on which the resource was changed (UTC).
    /// <br/>For Darwin Core, recommended best practice is to use an
    /// <br/>encoding scheme, such as ISO 8601:2004(E).
    /// </summary>
    public System.DateTimeOffset? Modified { get; set; }

    /// <summary>
    /// The name (or acronym) in use by the institution having
    /// <br/>ownership of the object(s) or information referred
    /// <br/>to in the record.
    /// </summary>
    public string? OwnerInstitutionCode { get; set; }

    /// <summary>
    /// Private Collection.
    /// </summary>
    public string? PrivateCollection { get; set; }

    /// <summary>
    /// Indicates whether the observation is sensitive and therefore protected.
    /// </summary>
    public bool Sensitive { get; set; }

    /// <summary>
    /// Public Collection.
    /// </summary>
    public string? PublicCollection { get; set; }

    /// <summary>
    /// A related resource that is referenced, cited,
    /// <br/>or otherwise pointed to by the described resource.
    /// </summary>
    public string? References { get; set; }

    /// <summary>
    /// A person or organization owning or
    /// <br/>managing rights over the resource.
    /// </summary>
    public string? RightsHolder { get; set; }

    /// <summary>
    /// Species collection label.
    /// </summary>
    public string? SpeciesCollectionLabel { get; set; }

    public VocabularyValue? Type { get; set; }

    public ArtportalenInternal? ArtportalenInternal { get; set; }

    /// <summary>
    /// Measurement or facts associated with the observation.
    /// </summary>
    public ICollection<ExtendedMeasurementOrFact>? MeasurementOrFacts { get; set; }

    /// <summary>
    /// Projects from Artportalen associated with the observation.
    /// </summary>
    public ICollection<Project>? Projects { get; set; }

    public ProjectsSummary? ProjectsSummary { get; set; }

    /// <summary>
    /// The date the observation was created (UTC).
    /// </summary>
    public System.DateTimeOffset Created { get; set; }

    public DataQuality? DataQuality { get; set; }

}


