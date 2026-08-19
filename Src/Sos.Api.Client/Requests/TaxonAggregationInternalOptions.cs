
namespace SOS.Api.Client.Requests;
/// <summary>Options for taxon aggregation.</summary>
public sealed class TaxonAggregationInternalOptions
{
    /// <summary>The explicit authorization role identifier.</summary>
    public int? RoleId { get; set; }

    /// <summary>The explicit calling application identifier.</summary>
    public string? AuthorizationApplicationIdentifier { get; set; }

    /// <summary>The number of buckets to skip.</summary>
    public int? Skip { get; set; }

    /// <summary>The maximum number of buckets to return.</summary>
    public int? Take { get; set; }

    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>The culture used to translate labels.</summary>
    public string TranslationCultureCode { get; set; } = "sv-SE";

    /// <summary>Controls whether the sensitive-observation index is searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Controls whether values from underlying taxa are included in parent-taxon buckets.</summary>
    public bool SumUnderlyingTaxa { get; set; }

    /// <summary>Controls whether server-side aggregation caching is bypassed.</summary>
    public bool? SkipCache { get; set; } = false;

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("translationCultureCode", TranslationCultureCode)
            .Add("sensitiveObservations", SensitiveObservations)
            .Add("sumUnderlyingTaxa", SumUnderlyingTaxa)
            .Add("skip", Skip)
            .Add("take", Take)
            .Add("skipCache", SkipCache)
            .ToString();
    }
}


