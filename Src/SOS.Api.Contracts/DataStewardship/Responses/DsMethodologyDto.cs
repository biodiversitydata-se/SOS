using System.ComponentModel.DataAnnotations;

namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// Describes a methodology used for a data-stewardship dataset.
/// </summary>
public sealed class DsMethodologyDto
{
    /// <summary>The methodology name.</summary>
    [Required] public string MethodologyName { get; set; } = null!;
    /// <summary>The methodology description.</summary>
    [Required] public string MethodologyDescription { get; set; } = null!;
    /// <summary>A persistent link to a description of the methodology.</summary>
    [Required] public string MethodologyLink { get; set; } = null!;
    /// <summary>A persistent link to the published species list for the dataset.</summary>
    [Required] public string SpeciesList { get; set; } = null!;
}
