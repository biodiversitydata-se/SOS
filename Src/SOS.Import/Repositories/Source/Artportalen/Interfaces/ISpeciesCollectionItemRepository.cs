using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities.Artportalen;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    public interface ISpeciesCollectionItemRepository
    {
        Task<IEnumerable<SpeciesCollectionItemEntity>> GetBySightingAsync(IEnumerable<int> sightingIds, bool live = false);
    }
}