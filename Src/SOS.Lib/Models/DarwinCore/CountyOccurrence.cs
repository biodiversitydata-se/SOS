namespace SOS.Lib.Models.DarwinCore;

/// <summary>
///     Taxon county occurrence
/// </summary>
public class CountyOccurrence
{
    public int Id { get; set; }
    public string County { get; set; }
    public string Status { get; set; }
}