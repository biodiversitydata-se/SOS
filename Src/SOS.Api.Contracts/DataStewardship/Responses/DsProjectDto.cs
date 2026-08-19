namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Identifies a project associated with a data-stewardship dataset.
/// </summary>
public sealed class DsProjectDto
{
    /// <summary>The unique project identifier.</summary>
    public string? ProjectID { get; set; }
    /// <summary>The project name.</summary>
    public string? ProjectCode { get; set; }
    /// <summary>The type of project in which the dataset was collected.</summary>
    public string? ProjectType { get; set; }
}
