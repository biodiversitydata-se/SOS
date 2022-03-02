using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface ISpeciesCollectionItemRepository
    {
        /// <summary>
        /// Get collection Items by sighting id's
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        Task<IEnumerable<SpeciesCollectionItemEntity>?> GetBySightingAsync(IEnumerable<int> sightingIds, bool live = false);
    }
}