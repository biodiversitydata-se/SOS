namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Describes a methodology used for a data-stewardship dataset.
/// </summary>
public sealed class DsMethodologyDto
{
    /// <summary>The methodology name.</summary>
    public string? MethodologyName { get; set; }
    /// <summary>The methodology description.</summary>
    public string? MethodologyDescription { get; set; }
    /// <summary>A persistent link to a description of the methodology.</summary>
    public string? MethodologyLink { get; set; }
    /// <summary>A persistent link to the published species list for the dataset.</summary>
    public string? SpeciesList { get; set; }
}
