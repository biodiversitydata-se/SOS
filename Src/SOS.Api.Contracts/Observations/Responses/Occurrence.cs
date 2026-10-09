using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Occurrence information about a species observation.
/// </summary>
public class Occurrence
{

    /// <summary>The activity of the observed organism.</summary>
    public VocabularyValue? Activity { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers
    /// <br/>(publication, global unique identifier, URI) of
    /// <br/>media associated with the Occurrence.
    /// </summary>
    public string? AssociatedMedia { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers of
    /// <br/>other Occurrence records and their associations to
    /// <br/>this Occurrence.
    /// </summary>
    public string? AssociatedOccurrences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers
    /// <br/>(publication, bibliographic reference, global unique
    /// <br/>identifier, URI) of literature associated with
    /// <br/>the Occurrence.
    /// </summary>
    public string? AssociatedReferences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers (publication, global unique identifier, URI) 
    /// <br/>of genetic sequence information associated with the Occurrence.
    /// </summary>
    public string? AssociatedSequences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers or
    /// <br/>names of taxa and their associations with the Occurrence.
    /// </summary>
    public string? AssociatedTaxa { get; set; }

    /// <summary>The behavior of the observed organism.</summary>
    public VocabularyValue? Behavior { get; set; }

    /// <summary>The biotope in which the organism was observed.</summary>
    public VocabularyValue? Biotope { get; set; }

    /// <summary>
    /// Description of biotope.
    /// </summary>
    public string? BiotopeDescription { get; set; }

    /// <summary>
    /// Bird nest activity.
    /// </summary>
    public int BirdNestActivityId { get; set; }

    /// <summary>
    /// An identifier (preferably unique) for the record
    /// <br/>within the data set or collection.
    /// </summary>
    public string? CatalogNumber { get; set; }

    /// <summary>
    /// An int32 identifier (preferably unique) for the record within the data set or collection.
    /// </summary>
    public int CatalogId { get; set; }

    /// <summary>
    /// The current state of a specimen with respect to the
    /// <br/>collection identified in collectionCode or collectionID.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string? Disposition { get; set; }

    /// <summary>How the organism became established at the location.</summary>
    public VocabularyValue? EstablishmentMeans { get; set; }

    /// <summary>
    /// The number of individuals represented present
    /// <br/>at the time of the Occurrence.
    /// </summary>
    public string? IndividualCount { get; set; }

    /// <summary>
    /// Indicates if this species occurrence is natural or
    /// <br/>if it is a result of human activity.
    /// </summary>
    public bool IsNaturalOccurrence { get; set; }

    /// <summary>
    /// Indicates if this observation is a never found observation.
    /// <br/>"Never found observation" is an observation that says
    /// <br/>that the specified species was not found in a location
    /// <br/>deemed appropriate for the species.
    /// </summary>
    public bool IsNeverFoundObservation { get; set; }

    /// <summary>
    /// Indicates if this observation is a
    /// <br/>not rediscovered observation.
    /// <br/>"Not rediscovered observation" is an observation that says
    /// <br/>that the specified species was not found in a location
    /// <br/>where it has previously been observed.
    /// </summary>
    public bool IsNotRediscoveredObservation { get; set; }

    /// <summary>
    /// Indicates if this observation is a positive observation.
    /// <br/>"Positive observation" is a normal observation indicating
    /// <br/>that a species has been seen at a specified location.
    /// </summary>
    public bool IsPositiveObservation { get; set; }

    /// <summary>The life stage of the observed organism.</summary>
    public VocabularyValue? LifeStage { get; set; }

    /// <summary>
    /// Media associated with the observation
    /// </summary>
    public ICollection<Multimedia>? Media { get; set; }

    /// <summary>
    /// An identifier for the Occurrence (as opposed to a particular digital record of the occurrence).
    /// <br/>In the absence of a persistent global unique identifier, construct one from a combination of
    /// <br/>identifiers in the record that will most closely make the occurrenceID globally unique.
    /// </summary>
    public string? OccurrenceId { get; set; }

    /// <summary>
    /// Comments or notes about the Occurrence.
    /// </summary>
    public string? OccurrenceRemarks { get; set; }

    /// <summary>The presence or absence of the organism at the location.</summary>
    public VocabularyValue? OccurrenceStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of previous or
    /// <br/>alternate fully qualified catalog numbers or other
    /// <br/>human-used identifiers for the same Occurrence,
    /// <br/>whether in the current or any other data set or collection.
    /// </summary>
    public string? OtherCatalogNumbers { get; set; }

    /// <summary>
    /// A number or enumeration value for the quantity of organisms.
    /// <br/>A dwc:organismQuantity must have a corresponding dwc:organismQuantityType.
    /// </summary>
    public string? OrganismQuantity { get; set; }

    /// <summary>
    /// Organism quantity used in aggregations
    /// </summary>
    public int? OrganismQuantityAggregation { get; set; }

    /// <summary>
    /// The quantity of organisms as integer. This field is necessary because we want to be able to do range-querys against quantities.
    /// </summary>
    public int? OrganismQuantityInt { get; set; }

    /// <summary>The unit associated with the reported organism quantity.</summary>
    public VocabularyValue? OrganismQuantityUnit { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of preparations
    /// <br/>and preservation methods for a specimen.
    /// </summary>
    public string? Preparations { get; set; }

    /// <summary>
    /// Information about how protected information about a species is in Sweden.
    /// <br/>This is a value between 1 to 5.
    /// <br/>1 indicates public access and 5 is the highest used security level.
    /// </summary>
    public int SensitivityCategory { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people,
    /// <br/>groups, or organizations responsible for recording the
    /// <br/>original Occurrence. The primary collector or observer,
    /// <br/>especially one who applies a personal identifier
    /// <br/>(recordNumber), should be listed first.
    /// </summary>
    public string? RecordedBy { get; set; }

    /// <summary>
    /// An identifier given to the Occurrence at the time it was
    /// <br/>recorded. Often serves as a link between field notes and
    /// <br/>an Occurrence record, such as a specimen collector's number.
    /// </summary>
    public string? RecordNumber { get; set; }

    /// <summary>
    /// Name of the person that reported the species observation.
    /// </summary>
    public string? ReportedBy { get; set; }

    /// <summary>
    /// Date and time when the species observation was reported.
    /// <br/>Observation search responses and GeoJSON observation exports use Swedish time (Europe/Stockholm),
    /// with the UTC offset at that date: +01:00 in winter or +02:00 in summer.
    /// </summary>
    /// <example>2026-07-15T12:00:00+02:00</example>
    public System.DateTimeOffset? ReportedDate { get; set; }

    /// <summary>The reproductive condition of the observed organism.</summary>
    public VocabularyValue? ReproductiveCondition { get; set; }

    /// <summary>The sex of the observed organism.</summary>
    public VocabularyValue? Sex { get; set; }

    /// <summary>The substrate on which the organism was observed.</summary>
    public Substrate? Substrate { get; set; }

    /// <summary>
    /// URL to occurrence.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The reported length.
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// The reported weight.
    /// </summary>
    public int? Weight { get; set; }

}


