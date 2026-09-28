using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;

/// <summary>Options for retrieving one observation.</summary>
public sealed class ObservationByIdOptions : SosRoleRequestOptions
{
    /// <summary>The observation fields included in the response.</summary>
    public OutputFieldSet OutputFieldSet { get; set; } = OutputFieldSet.Minimum;

    /// <summary>The culture used to translate labels.</summary>
    public string TranslationCultureCode { get; set; } = "sv-SE";

    /// <summary>Controls the use of the sensitive-observation index.</summary>
    [Obsolete("Use ProtectionFilter instead.")]
    public bool SensitiveObservations { get; set; }

    /// <summary>
    /// Overrides SensitiveObservations
    /// </summary>
    public ProtectionFilterDto ProtectionFilter { get; set; } = ProtectionFilterDto.Public;

    /// <summary>Controls resolution of generalized observations when the caller is authorized.</summary>
    public bool ResolveGeneralizedObservations { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .AddEnum("outputFieldSet", OutputFieldSet)
            .Add("translationCultureCode", TranslationCultureCode)
            .Add("sensitiveObservations", SensitiveObservations)
            .AddEnum("protectionFilter", ProtectionFilter)
            .Add("resolveGeneralizedObservations", ResolveGeneralizedObservations)
            .ToString();
    }
}
