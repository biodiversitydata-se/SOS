using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Information about a species observation.
/// </summary>
public class Observation
{

    /// <summary>The geographic generalization status of the observation.</summary>
    public DiffusionStatus DiffusionStatus { get; set; }

    /// <summary>
    /// Indicates whether the location is generalized.
    /// </summary>
    public bool IsGeneralized { get; set; }

    /// <summary>
    /// Indicates whether there exists a generalized location in other index.
    /// </summary>
    public bool HasGeneralizedObservationInOtherIndex { get; set; }

    /// <summary>Information about the observation event, including its date and sampling details.</summary>
    public Event? Event { get; set; }

    /// <summary>Data-stewardship dataset and event information associated with the observation.</summary>
    public DataStewardshipInfo? DataStewardship { get; set; }

    /// <summary>Geological context associated with the observation.</summary>
    public GeologicalContext? GeologicalContext { get; set; }

    /// <summary>Species identification and verification information.</summary>
    public Identification? Identification { get; set; }

    /// <summary>The geographic location of the observation.</summary>
    public Location? Location { get; set; }

    /// <summary>Information about the material sample associated with the observation.</summary>
    public MaterialSample? MaterialSample { get; set; }

    /// <summary>Information about the organism's occurrence, quantity and reported attributes.</summary>
    public Occurrence? Occurrence { get; set; }

    /// <summary>Information identifying the organism or group of organisms.</summary>
    public Organism? Organism { get; set; }

    /// <summary>Taxonomic information for the observed organism.</summary>
    public Taxon? Taxon { get; set; }

    /// <summary>The access rights associated with the observation.</summary>
    public VocabularyValue? AccessRights { get; set; }

    /// <summary>The nature or source of the observation record.</summary>
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

    /// <summary>The code of the institution having custody of the record or specimen.</summary>
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
    /// <example>
    ///     http://creativecommons.org/publicdomain/zero/1.0/legalcode,
    ///     http://creativecommons.org/licenses/by/4.0/legalcode
    /// </example>
    public string? License { get; set; }

    /// <summary>
    /// The most recent date-time on which the resource was changed.
    /// <br/>Observation search responses and GeoJSON observation exports use Swedish time (Europe/Stockholm),
    /// with the UTC offset at that date: +01:00 in winter or +02:00 in summer.
    /// <br/>For Darwin Core, recommended best practice is to use an
    /// <br/>encoding scheme, such as ISO 8601:2004(E).
    /// </summary>
    /// <example>2026-07-15T12:00:00+02:00</example>
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
    /// Indicates whether the observation is protected. Replaced by <see cref="Sensitive"/>.
    /// </summary>
    [Obsolete("Replaced by Sensitive")]
    public bool Protected => Sensitive;

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

    /// <summary>The nature or genre of the resource represented by the record.</summary>
    public VocabularyValue? Type { get; set; }

    /// <summary>Additional internal Artportalen information associated with the observation.</summary>
    public ArtportalenInternal? ArtportalenInternal { get; set; }

    /// <summary>
    /// Measurement or facts associated with the observation.
    /// </summary>
    public ICollection<ExtendedMeasurementOrFact>? MeasurementOrFacts { get; set; }

    /// <summary>
    /// Projects from Artportalen associated with the observation.
    /// </summary>
    public ICollection<Project>? Projects { get; set; }

    /// <summary>Summary information about projects associated with the observation.</summary>
    public ProjectsSummary? ProjectsSummary { get; set; }

    /// <summary>
    /// The date and time the observation was created.
    /// <br/>Observation search responses and GeoJSON observation exports use Swedish time (Europe/Stockholm),
    /// with the UTC offset at that date: +01:00 in winter or +02:00 in summer.
    /// </summary>
    /// <example>2026-01-15T11:00:00+01:00</example>
    public System.DateTimeOffset Created { get; set; }

    /// <summary>
    /// MongoDB identifier exposed by the existing API contract.
    /// </summary>
    public int MongoDbId { get; set; }

    /// <summary>Data-quality information associated with the observation.</summary>
    public DataQuality? DataQuality { get; set; }

}


