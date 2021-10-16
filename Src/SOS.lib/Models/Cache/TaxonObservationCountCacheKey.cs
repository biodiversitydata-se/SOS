using System;
using SOS.Lib.Cache;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Models.Cache
{
    /// <summary>
    /// Cache key for count number of observations for a specific taxon.
    /// </summary>
    public class TaxonObservationCountCacheKey
    {
        public int TaxonId { get; set; }
        public bool IncludeUnderlyingTaxa { get; set; }
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }
        public AreaType? AreaType  { get; set; }
        public string FeatureId { get; set; }

        protected bool Equals(TaxonObservationCountCacheKey other)
        {
            return TaxonId == other.TaxonId && IncludeUnderlyingTaxa == other.IncludeUnderlyingTaxa && FromYear == other.FromYear && ToYear == other.ToYear && AreaType == other.AreaType && FeatureId == other.FeatureId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TaxonObservationCountCacheKey)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TaxonId, IncludeUnderlyingTaxa, FromYear, ToYear, AreaType, FeatureId);
        }

        public static TaxonObservationCountCacheKey Create(TaxonObservationCountSearch taxonObservationCountSearch)
        {
            return new TaxonObservationCountCacheKey
            {
                AreaType = taxonObservationCountSearch.AreaType,
                FeatureId = taxonObservationCountSearch.FeatureId,
                FromYear = taxonObservationCountSearch.FromYear,
                ToYear = taxonObservationCountSearch.ToYear,
                IncludeUnderlyingTaxa = taxonObservationCountSearch.IncludeUnderlyingTaxa
            };
        }

        public static TaxonObservationCountCacheKey Create(TaxonObservationCountSearch taxonObservationCountSearch, int taxonId)
        {
            return new TaxonObservationCountCacheKey
            {
                TaxonId = taxonId,
                AreaType = taxonObservationCountSearch.AreaType,
                FeatureId = taxonObservationCountSearch.FeatureId,
                FromYear = taxonObservationCountSearch.FromYear,
                ToYear = taxonObservationCountSearch.ToYear,
                IncludeUnderlyingTaxa = taxonObservationCountSearch.IncludeUnderlyingTaxa
            };
        }
    }
}