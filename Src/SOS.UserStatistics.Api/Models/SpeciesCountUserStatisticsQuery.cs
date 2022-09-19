namespace SOS.UserStatistics.Api.Models;

public class SpeciesCountUserStatisticsQuery
{
    public int? TaxonId { get; set; }
    public int? Year { get; set; }
    public SpeciesGroup? SpeciesGroup { get; set; }
    public AreaType? AreaType { get; set; }
    public string FeatureId { get; set; }
    public int? SiteId { get; set; }
    public int? ProjectId { get; set; }
    public bool IncludeOtherAreasSpeciesCount { get; set; }
    public string SortByFeatureId { get; set; }

    public string CacheKey => ToString();

    public override string ToString()
    {
        return $"{nameof(TaxonId)}: {TaxonId}, {nameof(Year)}: {Year}, {nameof(SpeciesGroup)}: {SpeciesGroup}, " +
            $"{nameof(AreaType)}: {AreaType}, {nameof(FeatureId)}: {FeatureId}, {nameof(SiteId)}: {SiteId}, " +
            $"{nameof(ProjectId)}: {ProjectId}, {nameof(IncludeOtherAreasSpeciesCount)}: {IncludeOtherAreasSpeciesCount}, " +
            $"{nameof(SortByFeatureId)}: {SortByFeatureId}";
    }

    protected bool Equals(SpeciesCountUserStatisticsQuery other)
    {
        return TaxonId == other.TaxonId && Year == other.Year && SpeciesGroup == other.SpeciesGroup 
            && AreaType == other.AreaType && FeatureId == other.FeatureId && SiteId == other.SiteId 
            && ProjectId == other.ProjectId && IncludeOtherAreasSpeciesCount == other.IncludeOtherAreasSpeciesCount 
            && SortByFeatureId == other.SortByFeatureId;
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
        var hashCode = new HashCode();
        hashCode.Add(TaxonId);
        hashCode.Add(Year);
        hashCode.Add(SpeciesGroup);
        hashCode.Add(AreaType);
        hashCode.Add(FeatureId);
        hashCode.Add(SiteId);
        hashCode.Add(ProjectId);
        hashCode.Add(IncludeOtherAreasSpeciesCount);
        hashCode.Add(SortByFeatureId);
        return hashCode.ToHashCode();
    }

    public SpeciesCountUserStatisticsQuery Clone()
    {
        return (SpeciesCountUserStatisticsQuery) this.MemberwiseClone();
    }
}