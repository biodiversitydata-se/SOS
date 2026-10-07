namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Response output settings
/// </summary>
public class OutputFilterDto
{

    /// <summary>
    /// Predefined set of observation fields to return.
    /// When omitted, Minimum is used unless Fields are specified.
    /// None adds no predefined fields; only Fields are returned.
    /// </summary>
    public OutputFieldSet? FieldSet { get; set; }

    /// <summary>
    /// This parameter allows you to decide what fields should be returned, using a projection.
    /// <br/>When FieldSet is omitted or None, only these fields are returned; otherwise they are added to the selected field set.
    /// <br/>When neither Fields nor FieldSet is specified, the Minimum field set is used.
    /// <br/>For example, to retrieve only basic observation data, specify:
    /// <br/>["event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus"].
    /// </summary>
    public ICollection<string>? Fields { get; set; }

}


