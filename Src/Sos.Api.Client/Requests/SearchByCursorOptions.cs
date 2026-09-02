using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;
/// <summary>Options for cursor-based observation search.</summary>
public sealed class SearchByCursorOptions
{
    /// <summary>The explicit authorization role identifier.</summary>
    public int? RoleId { get; set; }

    /// <summary>The explicit calling application identifier.</summary>
    public string? AuthorizationApplicationIdentifier { get; set; }

    /// <summary>The maximum number of records to return.</summary>
    public int Take { get; set; } = 1000;

    /// <summary>The cursor returned by the preceding page.</summary>
    public string? Cursor { get; set; }

    /// <summary>The property path used for sorting.</summary>
    public string SortBy { get; set; } = "modified";

    /// <summary>The sort direction.</summary>
    public SearchSortOrder SortOrder { get; set; } = SearchSortOrder.Asc;

    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>The culture used to translate labels.</summary>
    public string TranslationCultureCode { get; set; } = "sv-SE";

    /// <summary>Controls whether the sensitive-observation index is searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("take", Take)
            .Add("sortBy", SortBy)
            .AddEnum("sortOrder", SortOrder)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("translationCultureCode", TranslationCultureCode)
            .Add("sensitiveObservations", SensitiveObservations)
            .Add("cursor", string.IsNullOrWhiteSpace(Cursor) ? null : Cursor)
            .ToString();
    }
}


