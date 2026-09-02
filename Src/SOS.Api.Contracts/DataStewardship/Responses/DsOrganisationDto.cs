namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Identifies an organisation associated with a data-stewardship dataset.
/// </summary>
public sealed class DsOrganisationDto
{
    /// <summary>The organisation name.</summary>
    public string? OrganisationCode { get; set; }
    /// <summary>The organisation's identifier.</summary>
    public string? OrganisationID { get; set; }
}
