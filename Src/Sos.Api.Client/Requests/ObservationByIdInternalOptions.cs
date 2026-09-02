using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;

/// <summary>Options for retrieving one observation from an internal endpoint.</summary>
public sealed class ObservationByIdInternalOptions : SosRoleRequestOptions
{
    /// <summary>An occurrence identifier supplied as a query parameter.</summary>
    public string? OccurrenceId { get; set; }

    /// <summary>The observation fields included in the response.</summary>
    public OutputFieldSet OutputFieldSet { get; set; } = OutputFieldSet.Minimum;

    /// <summary>The culture used to translate labels.</summary>
    public string TranslationCultureCode { get; set; } = "sv-SE";

    /// <summary>Controls the use of the sensitive-observation index.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Controls resolution of generalized observations when the caller is authorized.</summary>
    public bool ResolveGeneralizedObservations { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .AddEnum("outputFieldSet", OutputFieldSet)
            .Add("translationCultureCode", TranslationCultureCode)
            .Add("sensitiveObservations", SensitiveObservations)
            .Add("resolveGeneralizedObservations", ResolveGeneralizedObservations)
            .Add("occurrenceId", string.IsNullOrWhiteSpace(OccurrenceId) ? null : OccurrenceId)
            .ToString();
    }
}


