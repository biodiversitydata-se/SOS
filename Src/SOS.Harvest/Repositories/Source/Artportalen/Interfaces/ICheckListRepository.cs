using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Check list repository interface
    /// </summary>
    public interface IChecklistRepository : IBaseRepository<IChecklistRepository>
    {
        /// <summary>
        /// Get all taxon id's for passed checklists
        /// </summary>
        /// <param name="checklistIds"></param>
        /// <returns></returns>
        Task<IDictionary<int, ICollection<int>>> GetChecklistsTaxonIdsAsync(
            IEnumerable<int> checklistIds);

        /// <summary>
        /// Get chunk of checklists from Artportalen
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        Task<IEnumerable<ChecklistEntity>> GetChunkAsync(int startId, int maxRows);

        /// <summary>
        ///     Get min and max id
        /// </summary>
        /// <returns></returns>
        Task<(int minId, int maxId)> GetIdSpanAsync();

        /// <summary>
        /// True if live data base should be used
        /// </summary>
        bool Live { get; set; }
    }
}