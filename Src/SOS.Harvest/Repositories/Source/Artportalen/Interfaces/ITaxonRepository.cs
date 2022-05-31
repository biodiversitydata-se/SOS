using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface ITaxonRepository : IBaseRepository<ITaxonRepository>
    {
        Task<IEnumerable<TaxonEntity>> GetAsync();
    }
}