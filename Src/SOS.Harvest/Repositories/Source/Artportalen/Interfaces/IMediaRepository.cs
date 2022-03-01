using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Area repository interface
    /// </summary>
    public interface IMediaRepository
    {
        /// <summary>
        ///  Get media connected to provided sightings
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        Task<IEnumerable<MediaEntity>> GetAsync(IEnumerable<int> sightingIds, bool live = false);
    }
}