using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Area repository interface
    /// </summary>
    public interface IDiaryEntryRepository : IBaseRepository<IDiaryEntryRepository>
    {
        /// <summary>
        ///     Get all diary entries
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<DiaryEntryEntity>> GetAsync();
    }
}