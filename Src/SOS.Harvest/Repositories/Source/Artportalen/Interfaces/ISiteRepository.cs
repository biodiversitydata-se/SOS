using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Site repository interface
    /// </summary>
    public interface ISiteRepository
    {
        /// <summary>
        /// Get areas connected to sites
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        Task<IDictionary<int, ICollection<AreaEntityBase>>?> GetSitesAreas(IEnumerable<int> siteIds, bool live = false);

        /// <summary>
        /// Get sites by id
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        Task<IEnumerable<SiteEntity>?> GetByIdsAsync(IEnumerable<int> ids, bool live = false);

        /// <summary>
        /// Get site geometry
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        Task<IDictionary<int, string>?> GetSitesGeometry(IEnumerable<int> siteIds, bool live = false);
    }
}