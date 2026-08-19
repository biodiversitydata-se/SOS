namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Measurement or facts for an observation or event.
/// </summary>
public class ExtendedMeasurementOrFact
{

    /// <summary>
    /// An identifier for the MeasurementOrFact (information pertaining
    /// <br/>to measurements, facts, characteristics, or assertions).
    /// <br/>May be a global unique identifier or an identifier specific to the data set.
    /// </summary>
    public string? MeasurementID { get; set; }

    /// <summary>
    /// The identifier of the occurrence the measurement or fact refers to.
    /// <br/>If not applicable, it should be left empty.
    /// </summary>
    public string? OccurrenceID { get; set; }

    /// <summary>
    /// The nature of the measurement, fact, characteristic, or assertion.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string? MeasurementType { get; set; }

    /// <summary>
    /// An identifier for the measurementType (global unique identifier, URI).
    /// <br/>The identifier should reference the measurementType in a vocabulary.
    /// </summary>
    public string? MeasurementTypeID { get; set; }

    /// <summary>
    /// The value of the measurement, fact, characteristic, or assertion.
    /// </summary>
    public string? MeasurementValue { get; set; }

    /// <summary>
    /// An identifier for facts stored in the column measurementValue (global unique identifier, URI).
    /// <br/>This identifier can reference a controlled vocabulary (e.g. for sampling instrument names,
    /// <br/>methodologies, life stages) or reference a methodology paper with a DOI. When the measurementValue
    /// <br/>refers to a value and not to a fact, the measurementvalueID has no meaning and should remain empty.
    /// </summary>
    public string? MeasurementValueID { get; set; }

    /// <summary>
    /// The description of the potential error associated with the measurementValue.
    /// </summary>
    public string? MeasurementAccuracy { get; set; }

    /// <summary>
    /// The units associated with the measurementValue.
    /// <br/>Recommended best practice is to use the International System of Units (SI).
    /// </summary>
    public string? MeasurementUnit { get; set; }

    /// <summary>
    /// An identifier for the measurementUnit (global unique identifier, URI).
    /// <br/>The identifier should reference the measurementUnit in a vocabulary.
    /// </summary>
    public string? MeasurementUnitID { get; set; }

    /// <summary>
    /// The date on which the MeasurementOrFact was made. Recommended best
    /// <br/>practice is to use an encoding scheme, such as ISO 8601:2004(E).
    /// </summary>
    public string? MeasurementDeterminedDate { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people, groups, or organizations
    /// <br/>who determined the value of the MeasurementOrFact.
    /// </summary>
    public string? MeasurementDeterminedBy { get; set; }

    /// <summary>
    /// A description of or reference to (publication, URI) the method or protocol used
    /// <br/>to determine the measurement, fact, characteristic, or assertion.
    /// </summary>
    public string? MeasurementMethod { get; set; }

    /// <summary>
    /// Comments or notes accompanying the MeasurementOrFact.
    /// </summary>
    public string? MeasurementRemarks { get; set; }

}


