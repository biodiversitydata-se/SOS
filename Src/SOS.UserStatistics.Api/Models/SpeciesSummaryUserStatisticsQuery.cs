namespace SOS.UserStatistics.Api.Models;

public class SpeciesSummaryUserStatisticsQuery
{
    public int? TaxonId { get; set; }
    public int? Year { get; set; }
    public SpeciesGroup? SpeciesGroup { get; set; }
    public AreaType? AreaType { get; set; }
    public string FeatureId { get; set; }

    public string CacheKey => ToString();

    public override string ToString()
    {
        return $"{nameof(TaxonId)}: {TaxonId}, " +
            $"{nameof(Year)}: {TaxonId}, " +
            $"{nameof(SpeciesGroup)}: " +
            $"{SpeciesGroup}, " +
            $"{nameof(AreaType)}: {AreaType}, " +
            $"{nameof(FeatureId)}: {FeatureId}, ";
    }

    protected bool Equals(SpeciesSummaryUserStatisticsQuery other)
    {
        return TaxonId == other.TaxonId
            && Year == other.Year
            && SpeciesGroup == other.SpeciesGroup
            && AreaType == other.AreaType
            && FeatureId == other.FeatureId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SpeciesSummaryUserStatisticsQuery) obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(TaxonId);
        hashCode.Add(Year);
        hashCode.Add(SpeciesGroup);
        hashCode.Add(AreaType);
        hashCode.Add(FeatureId);
        return hashCode.ToHashCode();
    }

    public SpeciesSummaryUserStatisticsQuery Clone()
    {
        return (SpeciesSummaryUserStatisticsQuery) this.MemberwiseClone();
    }
}