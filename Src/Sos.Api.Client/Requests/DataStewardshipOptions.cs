namespace SOS.Api.Client.Requests;

/// <summary>Options for retrieving data-stewardship datasets.</summary>
public sealed class DataStewardshipDatasetOptions : SosRoleRequestOptions
{
    /// <summary>The number of datasets to skip.</summary>
    public int Skip { get; set; }
    /// <summary>The maximum number of datasets to return.</summary>
    public int Take { get; set; } = 100;
    /// <summary>
    /// Controls whether dataset event identifiers are included by the observation-filter search endpoint.
    /// </summary>
    public bool IncludeEventIds { get; set; }
    /// <summary>
    /// Controls whether the observation-filter search endpoint validates the search filter.
    /// </summary>
    public bool ValidateSearchFilter { get; set; }
}
