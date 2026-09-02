using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Client.Requests;
/// <summary>Options for retrieving SOS data providers.</summary>
public sealed class DataProviderOptions
{
    /// <summary>Provider categories included in the result.</summary>
    public IEnumerable<DataProviderCategory>? Categories { get; set; }

    /// <summary>The culture used for translated provider names.</summary>
    public string CultureCode { get; set; } = "sv-SE";

    /// <summary>Controls whether providers without observations are included.</summary>
    public bool IncludeProvidersWithNoObservations { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("cultureCode", CultureCode)
            .Add("includeProvidersWithNoObservations", IncludeProvidersWithNoObservations)
            .AddValues("categories", Categories)
            .ToString();
    }
}


