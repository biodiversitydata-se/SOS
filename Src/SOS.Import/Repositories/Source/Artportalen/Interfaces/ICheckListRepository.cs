using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities.Artportalen;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Check list repository interface
    /// </summary>
    public interface ICheckListRepository
    {
        /// <summary>
        /// Get all taxon id's for passed check lists
        /// </summary>
        /// <param name="checkListIds"></param>
        /// <returns></returns>
        Task<IDictionary<int, ICollection<int>>> GetCheckListsTaxonIdsAsync(
            IEnumerable<int> checkListIds);

        /// <summary>
        /// Get chunk of check lists from Artportalen
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        Task<IEnumerable<CheckListEntity>> GetChunkAsync(int startId, int maxRows);

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