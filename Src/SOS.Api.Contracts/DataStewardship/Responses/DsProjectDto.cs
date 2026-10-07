using System.ComponentModel.DataAnnotations;

namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Identifies a project associated with a data-stewardship dataset.
/// </summary>
public sealed class DsProjectDto
{
    /// <summary>The unique project identifier.</summary>
    public string? ProjectID { get; set; }
    /// <summary>The project name.</summary>
    [Required] public string ProjectCode { get; set; } = null!;
    /// <summary>The type of project in which the dataset was collected.</summary>
    [Required] public DsProjectType? ProjectType { get; set; }
}
