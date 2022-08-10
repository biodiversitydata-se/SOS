using System;

namespace SOS.Lib.Models.Statistics;

public class PagedSpeciesCountUserStatisticsQuery : SpeciesCountUserStatisticsQuery
{
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public string SortBy { get; set; }

    public override string ToString()
    {
        return $"{nameof(Skip)}: {Skip}, {nameof(Take)}: {Take}, {nameof(SortBy)}: {SortBy}, {nameof(TaxonId)}: {TaxonId}, {nameof(Year)}: {Year}, {nameof(SpeciesGroup)}: {SpeciesGroup}, {nameof(AreaType)}: {AreaType}, {nameof(FeatureId)}: {FeatureId}, {nameof(SiteId)}: {SiteId}, {nameof(ProjectId)}: {ProjectId}, {nameof(IncludeOtherAreasSpeciesCount)}: {IncludeOtherAreasSpeciesCount}, {nameof(CacheKey)}: {CacheKey}";
    }

    protected bool Equals(PagedSpeciesCountUserStatisticsQuery other)
    {
        return Skip == other.Skip && Take == other.Take && SortBy == other.SortBy && TaxonId == other.TaxonId && Year == other.Year && SpeciesGroup == other.SpeciesGroup && AreaType == other.AreaType && FeatureId == other.FeatureId && SiteId == other.SiteId && ProjectId == other.ProjectId && IncludeOtherAreasSpeciesCount == other.IncludeOtherAreasSpeciesCount;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PagedSpeciesCountUserStatisticsQuery)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Skip);
        hashCode.Add(Take);
        hashCode.Add(SortBy);
        hashCode.Add(TaxonId);
        hashCode.Add(Year);
        hashCode.Add(SpeciesGroup);
        hashCode.Add(AreaType);
        hashCode.Add(FeatureId);
        hashCode.Add(SiteId);
        hashCode.Add(ProjectId);
        hashCode.Add(IncludeOtherAreasSpeciesCount);
        return hashCode.ToHashCode();
    }

    public static PagedSpeciesCountUserStatisticsQuery Create(SpeciesCountUserStatisticsQuery query, int? skip,
        int? take, string sortBy)
    {
        var pagedQuery = new PagedSpeciesCountUserStatisticsQuery();
        pagedQuery.Skip = skip;
        pagedQuery.Take = take;
        pagedQuery.SortBy = sortBy;
        pagedQuery.FeatureId = query.FeatureId;
        pagedQuery.SiteId = query.SiteId;
        pagedQuery.ProjectId = query.ProjectId;
        pagedQuery.IncludeOtherAreasSpeciesCount = query.IncludeOtherAreasSpeciesCount;
        pagedQuery.AreaType = query.AreaType;
        pagedQuery.Year = query.Year;
        pagedQuery.TaxonId = query.TaxonId;
        pagedQuery.SpeciesGroup = query.SpeciesGroup;

        return pagedQuery;
    }
}