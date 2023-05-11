using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    public interface IPersonRepository : IBaseRepository<IPersonRepository>
    {
        /// <summary>
        /// Get all persons
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PersonEntity>> GetAsync();

        /// <summary>
        /// Get one person
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<PersonEntity> GetByUserIdAsync(int id);
    }
}