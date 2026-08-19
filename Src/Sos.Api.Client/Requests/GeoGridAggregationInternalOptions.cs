
namespace SOS.Api.Client.Requests;
/// <summary>Options for geographic-grid aggregation.</summary>
public sealed class GeoGridAggregationInternalOptions
{
    /// <summary>The explicit authorization role identifier.</summary>
    public int? RoleId { get; set; }

    /// <summary>The explicit calling application identifier.</summary>
    public string? AuthorizationApplicationIdentifier { get; set; }

    /// <summary>The geographic grid zoom level.</summary>
    public int Zoom { get; set; } = 1;

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
            .Add("zoom", Zoom)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("translationCultureCode", TranslationCultureCode)
            .Add("sensitiveObservations", SensitiveObservations)
            .ToString();
    }
}


