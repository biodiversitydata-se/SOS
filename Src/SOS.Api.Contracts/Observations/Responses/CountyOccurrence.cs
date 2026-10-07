namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>
/// Taxon county occurrence.
/// </summary>
public class CountyOccurrence
{
    /// <summary>The county occurrence identifier.</summary>
    public int Id { get; set; }

    /// <summary>The county name.</summary>
    public string? County { get; set; }

    /// <summary>The taxon's occurrence status in the county.</summary>
    public string? Status { get; set; }
}
