using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    public interface ISightingRelationRepository
    {
        Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds);
    }
}
