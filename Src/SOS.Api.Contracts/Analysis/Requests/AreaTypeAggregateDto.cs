namespace SOS.Api.Contracts.Analysis.Requests;

/// <summary>
/// Area type used when aggregating observations by area.
/// </summary>
public enum AreaTypeAggregateDto
{
    /// <summary>Swedish municipality.</summary>
    Municipality = 1,
    /// <summary>Swedish major region (landsdel).</summary>
    CountryRegion = 13,
    /// <summary>Swedish province.</summary>
    Province = 16,
    /// <summary>Swedish county.</summary>
    County = 21,
    /// <summary>Atlas grid with 5 by 5 kilometre cells.</summary>
    Atlas5x5 = 29,
    /// <summary>Atlas grid with 10 by 10 kilometre cells.</summary>
    Atlas10x10 = 30
}
