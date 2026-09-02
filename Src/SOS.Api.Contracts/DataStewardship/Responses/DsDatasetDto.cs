namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Dataset metadata returned by the SOS DataStewardship endpoints.
/// Enum-backed server values are represented as strings to preserve their EnumMember JSON values.
/// </summary>
public sealed class DsDatasetDto
{
    /// <summary>The dataset identifier.</summary>
    public string? Identifier { get; set; }
    /// <summary>The dataset license.</summary>
    public string? License { get; set; }
    /// <summary>The dataset title.</summary>
    public string? Title { get; set; }
    /// <summary>Projects associated with the dataset.</summary>
    public IEnumerable<DsProjectDto>? Projects { get; set; }
    /// <summary>The environmental-monitoring programme area to which the dataset belongs.</summary>
    public string? ProgrammeArea { get; set; }
    /// <summary>The organisation that commissioned the data collection.</summary>
    public DsOrganisationDto? Assigner { get; set; }
    /// <summary>Organisations responsible for the data collection.</summary>
    public IEnumerable<DsOrganisationDto>? Creator { get; set; }
    /// <summary>The organisation responsible for the information in the dataset.</summary>
    public DsOrganisationDto? OwnerinstitutionCode { get; set; }
    /// <summary>The organisation or data host responsible for making the dataset accessible.</summary>
    public DsOrganisationDto? Publisher { get; set; }
    /// <summary>The name of the data stewardship within which the dataset is handled.</summary>
    public string? DataStewardship { get; set; }
    /// <summary>The purpose of the data collection.</summary>
    public string? Purpose { get; set; }
    /// <summary>The dataset description.</summary>
    public string? Description { get; set; }
    /// <summary>Methodologies used for the data collection.</summary>
    public IEnumerable<DsMethodologyDto>? Methodology { get; set; }
    /// <summary>The start of the dataset's temporal extent.</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>The end of the dataset's temporal extent.</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>The country in which the dataset's data was collected.</summary>
    public string? Spatial { get; set; }
    /// <summary>The extent to which the dataset is publicly available.</summary>
    public string? AccessRights { get; set; }
    /// <summary>Additional information about the access rights.</summary>
    public string? DescriptionAccessRights { get; set; }
    /// <summary>The language used for metadata.</summary>
    public string? Metadatalanguage { get; set; }
    /// <summary>The language used in the dataset.</summary>
    public string? Language { get; set; }
    /// <summary>Unique identifiers for survey events in the dataset, when requested by the caller.</summary>
    public IEnumerable<string>? EventIds { get; set; }
}
