
namespace SOS.Api.Client.Requests;
/// <summary>Options for an internal observation count request.</summary>
public sealed class CountInternalOptions
{
    /// <summary>The explicit authorization role identifier.</summary>
    public int? RoleId { get; set; }

    /// <summary>The explicit calling application identifier.</summary>
    public string? AuthorizationApplicationIdentifier { get; set; }

    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>Controls whether the sensitive-observation index is searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("sensitiveObservations", SensitiveObservations)
            .ToString();
    }
}


