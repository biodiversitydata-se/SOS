using System.Collections.Generic;

namespace SOS.Lib.Models.Diagnostics;

public class TaxonSummary
{
    public List<TaxonSummaryItem> Taxa { get; set; } = new List<TaxonSummaryItem>();
    public List<int> TaxonIds { get; set; } = new List<int>();
    public List<int> IsProtectedByLaw { get; set; } = new List<int>();
    public List<int> IsRedlisted { get; set; } = new List<int>();
    public List<int> IsInvasiveInSweden { get; set; } = new List<int>();
    public List<int> IsInvasiveInEu { get; set; } = new List<int>();
    public List<int> IsBirdDirective { get; set; } = new List<int>();
}