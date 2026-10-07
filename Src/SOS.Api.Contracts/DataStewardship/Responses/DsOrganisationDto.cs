using System.ComponentModel.DataAnnotations;

namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Identifies an organisation associated with a data-stewardship dataset.
/// </summary>
public sealed class DsOrganisationDto
{
    /// <summary>The organisation name.</summary>
    [Required] public string OrganisationCode { get; set; } = null!;
    /// <summary>The organisation's identifier.</summary>
    public string? OrganisationID { get; set; }
}
