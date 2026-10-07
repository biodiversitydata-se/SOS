namespace SOS.Api.Contracts.Checklists.Responses;

/// <summary>
/// Taxon trend result.
/// </summary>
public class TaxonTrendResult
{
    /// <summary>The taxon identifier.</summary>
    public int TaxonId { get; set; }
    /// <summary>The number of present observations divided by the number of checklists.</summary>
    public double Quotient { get; set; }
    /// <summary>The number of present observations.</summary>
    public int NrPresentObservations { get; set; }
    /// <summary>The number of absent observations.</summary>
    public int NrAbsentObservations { get; set; }
    /// <summary>The number of checklists.</summary>
    public int NrChecklists { get; set; }
}
