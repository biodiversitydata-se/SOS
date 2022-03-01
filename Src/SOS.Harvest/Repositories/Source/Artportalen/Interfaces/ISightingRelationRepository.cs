using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface ISightingRelationRepository
    {
        Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds, bool live = false);
    }
}