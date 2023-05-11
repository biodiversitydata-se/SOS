using System.ComponentModel;

namespace SOS.Blazor.Shared.Models;

public class ProvinceUserStatisticsItem
{
    /// <summary>
    /// UserId.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Number of species (taxa) the user has found.
    /// </summary>
    public int SpeciesCount { get; set; }

    /// <summary>
    /// Number of observations.
    /// </summary>
    public int ObservationCount { get; set; }

    [DisplayName("Skåne")]
    public int Province01SpeciesCount { get; set; }

    [DisplayName("Blekinge")]
    public int Province02SpeciesCount { get; set; }

    [DisplayName("Småland")]
    public int Province03SpeciesCount { get; set; }
        
    [DisplayName("Öland")]
    public int Province04SpeciesCount { get; set; }

    [DisplayName("Gotland")]
    public int Province05SpeciesCount { get; set; }

    public static ProvinceUserStatisticsItem Create(UserStatisticsItem userStatisticsItem)
    {
        var item = new ProvinceUserStatisticsItem();
        item.UserId = userStatisticsItem.UserId;
        item.SpeciesCount = userStatisticsItem.SpeciesCount;
        item.ObservationCount = userStatisticsItem.ObservationCount;
        int count;
        item.Province01SpeciesCount = userStatisticsItem.SpeciesCountByFeatureId.TryGetValue("1", out count) ? count : 0;
        item.Province02SpeciesCount = userStatisticsItem.SpeciesCountByFeatureId.TryGetValue("2", out count) ? count : 0;
        item.Province03SpeciesCount = userStatisticsItem.SpeciesCountByFeatureId.TryGetValue("3", out count) ? count : 0;
        item.Province04SpeciesCount = userStatisticsItem.SpeciesCountByFeatureId.TryGetValue("4", out count) ? count : 0;
        item.Province05SpeciesCount = userStatisticsItem.SpeciesCountByFeatureId.TryGetValue("5", out count) ? count : 0;

        return item;
    }
}