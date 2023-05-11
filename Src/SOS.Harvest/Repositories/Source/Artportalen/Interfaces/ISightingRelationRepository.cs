using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface ISightingRelationRepository : IBaseRepository<ISightingRelationRepository>
    {
        Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds);
    }
}