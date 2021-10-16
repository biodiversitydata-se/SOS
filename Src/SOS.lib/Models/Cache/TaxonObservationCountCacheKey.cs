using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Cache;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
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
        public string DataProviderIds { get; set; }

        protected bool Equals(TaxonObservationCountCacheKey other)
        {
            return TaxonId == other.TaxonId && IncludeUnderlyingTaxa == other.IncludeUnderlyingTaxa && FromYear == other.FromYear && ToYear == other.ToYear && AreaType == other.AreaType && FeatureId == other.FeatureId && DataProviderIds == other.DataProviderIds;
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
            return HashCode.Combine(TaxonId, IncludeUnderlyingTaxa, FromYear, ToYear, AreaType, FeatureId, DataProviderIds);
        }

        public static TaxonObservationCountCacheKey Create(TaxonObservationCountSearch taxonObservationCountSearch)
        {
            return new TaxonObservationCountCacheKey
            {
                AreaType = taxonObservationCountSearch.AreaType,
                FeatureId = taxonObservationCountSearch.FeatureId,
                FromYear = taxonObservationCountSearch.FromYear,
                ToYear = taxonObservationCountSearch.ToYear,
                IncludeUnderlyingTaxa = taxonObservationCountSearch.IncludeUnderlyingTaxa,
                DataProviderIds = taxonObservationCountSearch.DataProviderIds != null && taxonObservationCountSearch.DataProviderIds.Any() ? string.Join(",", taxonObservationCountSearch.DataProviderIds?.OrderBy(m => m)) : null
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
                IncludeUnderlyingTaxa = taxonObservationCountSearch.IncludeUnderlyingTaxa,
                DataProviderIds = taxonObservationCountSearch.DataProviderIds != null && taxonObservationCountSearch.DataProviderIds.Any() ? string.Join(",", taxonObservationCountSearch.DataProviderIds?.OrderBy(m => m)) : null
            };
        }
    }
}