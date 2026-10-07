using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;
/// <summary>Options for a paged internal observation search.</summary>
public sealed class ObservationSearchOptions : SosRoleRequestOptions
{
    /// <summary>The number of records to skip.</summary>
    public int Skip { get; set; }

    /// <summary>The maximum number of records to return.</summary>
    public int Take { get; set; } = 100;

    /// <summary>The property path used for sorting.</summary>
    public string SortBy { get; set; } = string.Empty;

    /// <summary>The sort direction.</summary>
    public SearchSortOrder SortOrder { get; set; } = SearchSortOrder.Asc;

    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>The culture used to translate labels.</summary>
    public string TranslationCultureCode { get; set; } = "sv-SE";

    /// <summary>Controls whether the sensitive-observation index is searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>The response format.</summary>
    public OutputFormatDto OutputFormat { get; set; } = OutputFormatDto.Json;

    /// <summary>Controls whether polygon locations are included.</summary>
    public bool IncludePolygonLocations { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        var query = new SosQueryBuilder()
            .Add("skip", Skip)
            .Add("take", Take)
            .Add("sortBy", SortBy)
            .AddEnum("sortOrder", SortOrder)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("translationCultureCode", TranslationCultureCode)
            .Add("sensitiveObservations", SensitiveObservations)
            .AddEnum("outputFormat", OutputFormat);
        if (IncludePolygonLocations)
        {
            query.Add("includePolygonLocations", true);
        }

        return query.ToString();
    }
}


