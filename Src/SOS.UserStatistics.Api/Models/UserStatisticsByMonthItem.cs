namespace SOS.UserStatistics.Api.Models;

public class UserStatisticsByMonthItem
{
    /// <summary>
    /// UserId.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Species count by Month.
    /// </summary>
    public Dictionary<int, int> SpeciesCountByMonth { get; set; }

    /// <summary>
    /// Species count by Year and Month.
    /// </summary>
    public Dictionary<(int Year, int Month), int> SpeciesCountByYearAndMonth { get; set; }
}