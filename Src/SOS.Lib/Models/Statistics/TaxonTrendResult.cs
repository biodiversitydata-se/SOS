namespace SOS.Lib.Models.Statistics;

/// <summary>
/// Taxon trend result.
/// </summary>
public class TaxonTrendResult
{
    public int TaxonId { get; set; }
    public double Quotient { get; set; }
    public int NrPresentObservations { get; set; }
    public int NrAbsentObservations { get; set; }
    public int NrChecklists { get; set; }
}