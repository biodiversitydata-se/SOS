using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface ISpeciesCollectionItemRepository
    {
        Task<IEnumerable<SpeciesCollectionItemEntity>> GetBySightingAsync(IEnumerable<int> sightingIds, bool live = false);
    }
}