namespace SOS.UserStatistics.Api.Models;

public class SpeciesSummaryItem
{
    /// <summary>
    /// UserId.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Taxon id
    /// </summary>
    public int TaxonId { get; set; }

    /// <summary>
    /// First date when Taxon was found
    /// </summary>
    public DateOnly FirstFoundDate { get; set; }

    public UserStatisticsItem Clone()
    {
        return (UserStatisticsItem)MemberwiseClone();
    }
}