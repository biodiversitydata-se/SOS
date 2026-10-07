namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Filters that only affect observations from Artportalen.
/// </summary>
public class ArtportalenFilterDto
{
    /// <summary>
    /// Controls which Artportalen observation types are included in the result.
    /// </summary>
    public SightingTypeFilterDto TypeFilter { get; set; } = SightingTypeFilterDto.DoNotShowMerged;
}
