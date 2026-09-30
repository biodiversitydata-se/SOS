namespace SOS.Api.Client.Requests;
/// <summary>Options for retrieving SOS data provider eml.</summary>
public sealed class DataProviderEmlOptions
{
    /// <summary>The culture used for translated provider names.</summary>
    public string CultureCode { get; set; } = "sv-SE";


    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("cultureCode", CultureCode)
            .ToString();
    }
}


