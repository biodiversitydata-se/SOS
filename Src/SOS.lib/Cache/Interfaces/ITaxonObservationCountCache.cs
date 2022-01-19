using SOS.Lib.Models.Cache;

namespace SOS.Lib.Cache.Interfaces
{
    /// <summary>
    /// TaxonObservationCountCache interface.
    /// </summary>
    public interface ITaxonObservationCountCache
    {
        /// <summary>
        /// Add taxon observation count value.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="taxonCount"></param>
        public void Add(TaxonObservationCountCacheKey cacheKey, TaxonCount taxonCount);
        
        /// <summary>
        /// Try get cached count.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="taxonCount"></param>
        /// <returns></returns>
        public bool TryGetCount(TaxonObservationCountCacheKey cacheKey, out TaxonCount taxonCount);
    }
}