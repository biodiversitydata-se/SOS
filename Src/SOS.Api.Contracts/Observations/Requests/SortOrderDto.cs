namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Field and direction used to sort search results.</summary>
public class SortOrderDto
{

    /// <summary>
    /// Name of field to sort by
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>Ascending or descending sort direction.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SearchSortOrder SortOrder { get; set; }

}


