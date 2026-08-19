namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Response output settings
/// </summary>
public class OutputFilterDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OutputFieldSet FieldSet { get; set; }

    /// <summary>
    /// This parameter allows you to decide what fields should be returned, using a projection.
    /// <br/>Omit this parameter and you will receive the complete collection of fields.
    /// <br/>For example, to retrieve only basic observation data, specify:
    /// <br/>["event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus"].
    /// </summary>
    public ICollection<string> Fields { get; set; }

}


