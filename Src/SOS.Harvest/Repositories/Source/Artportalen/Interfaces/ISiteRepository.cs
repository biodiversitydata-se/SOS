using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Site repository interface
    /// </summary>
    public interface ISiteRepository : IBaseRepository<ISiteRepository>
    {
        /// <summary>
        /// Get areas connected to sites
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        Task<IDictionary<int, ICollection<AreaEntityBase>>?> GetSitesAreas(IEnumerable<int> siteIds);

        /// <summary>
        /// Get sites by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<SiteEntity>?> GetByIdsAsync(IEnumerable<int> ids);

        /// <summary>
        /// Get id from sites connected to more than one sighting
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<int>> GetFreqventlyUsedIdsAsync();

        /// <summary>
        /// Get site geometry
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        Task<IDictionary<int, string>?> GetSitesGeometry(IEnumerable<int> siteIds);
    }
}