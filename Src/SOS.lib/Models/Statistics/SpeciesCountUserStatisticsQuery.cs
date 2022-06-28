using System;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Statistics;

public class SpeciesCountUserStatisticsQuery
{
    public int? TaxonId { get; set; }
    public int? Year { get; set; }
    public SpeciesGroup? SpeciesGroup { get; set; }
    public AreaType? AreaType { get; set; }
    public string FeatureId { get; set; }
    public int? SiteId { get; set; }

    public string CacheKey => this.ToString();
    public override string ToString()
    {
        return $"{nameof(TaxonId)}: {TaxonId}, {nameof(Year)}: {Year}, {nameof(SpeciesGroup)}: {SpeciesGroup}, {nameof(AreaType)}: {AreaType}, {nameof(FeatureId)}: {FeatureId}, {nameof(SiteId)}: {SiteId}";
    }

    protected bool Equals(SpeciesCountUserStatisticsQuery other)
    {
        return TaxonId == other.TaxonId && Year == other.Year && SpeciesGroup == other.SpeciesGroup && AreaType == other.AreaType && FeatureId == other.FeatureId && SiteId == other.SiteId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SpeciesCountUserStatisticsQuery) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TaxonId, Year, SpeciesGroup, AreaType, FeatureId, SiteId);
    }
}