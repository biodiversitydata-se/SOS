using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface IPersonRepository : IBaseRepository<IPersonRepository>
    {
        Task<IEnumerable<PersonEntity>> GetAsync();
    }
}