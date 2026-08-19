namespace SOS.Api.Contracts.Observations.Requests;
public class SortOrderDto
{

    /// <summary>
    /// Name of field to sort by
    /// </summary>
    public string? SortBy { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SearchSortOrder SortOrder { get; set; }

}


