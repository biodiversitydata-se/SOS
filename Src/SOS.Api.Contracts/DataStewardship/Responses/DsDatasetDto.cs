using System.ComponentModel.DataAnnotations;

namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Dataset metadata returned by the SOS DataStewardship endpoints.
/// Programme area, purpose and access rights use the corresponding server enum types.
/// </summary>
public sealed class DsDatasetDto
{
    /// <summary>The dataset identifier.</summary>
    [Required] public string Identifier { get; set; } = null!;
    /// <summary>The dataset license.</summary>
    [Required] public string License { get; set; } = null!;
    /// <summary>The dataset title.</summary>
    [Required] public string Title { get; set; } = null!;
    /// <summary>Projects associated with the dataset.</summary>
    public IEnumerable<DsProjectDto>? Projects { get; set; }
    /// <summary>The environmental-monitoring programme area to which the dataset belongs.</summary>
    [Required] public DsProgrammeArea? ProgrammeArea { get; set; }
    /// <summary>The organisation that commissioned the data collection.</summary>
    [Required] public DsOrganisationDto Assigner { get; set; } = null!;
    /// <summary>Organisations responsible for the data collection.</summary>
    [Required] public IEnumerable<DsOrganisationDto> Creator { get; set; } = null!;
    /// <summary>The organisation responsible for the information in the dataset.</summary>
    [Required] public DsOrganisationDto OwnerinstitutionCode { get; set; } = null!;
    /// <summary>The organisation or data host responsible for making the dataset accessible.</summary>
    [Required] public DsOrganisationDto Publisher { get; set; } = null!;
    /// <summary>The name of the data stewardship within which the dataset is handled.</summary>
    [Required] public string DataStewardship { get; set; } = null!;
    /// <summary>The purpose of the data collection.</summary>
    [Required] public DsPurpose? Purpose { get; set; }
    /// <summary>The dataset description.</summary>
    [Required] public string Description { get; set; } = null!;
    /// <summary>Methodologies used for the data collection.</summary>
    [Required] public IEnumerable<DsMethodologyDto> Methodology { get; set; } = null!;
    /// <summary>The start of the dataset's temporal extent.</summary>
    [Required] public DateTime? StartDate { get; set; }
    /// <summary>The end of the dataset's temporal extent.</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>The country in which the dataset's data was collected.</summary>
    [Required] public string Spatial { get; set; } = null!;
    /// <summary>The extent to which the dataset is publicly available.</summary>
    [Required] public DsAccessRights? AccessRights { get; set; }
    /// <summary>Additional information about the access rights.</summary>
    public string? DescriptionAccessRights { get; set; }
    /// <summary>The language used for metadata.</summary>
    [Required] public string Metadatalanguage { get; set; } = null!;
    /// <summary>The language used in the dataset.</summary>
    [Required] public string Language { get; set; } = null!;
    /// <summary>Unique identifiers for survey events in the dataset, when requested by the caller.</summary>
    public IEnumerable<string>? EventIds { get; set; }
}
