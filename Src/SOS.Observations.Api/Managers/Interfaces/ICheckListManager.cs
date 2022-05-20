using System.Threading.Tasks;
using SOS.Lib.Models.Processed.CheckList;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Statistics;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public interface ICheckListManager
    {
        /// <summary>
        /// Calculate taxon trend
        /// </summary>
        /// <param name="observationFilter"></param>
        /// <param name="checkListSearchFilter"></param>
        /// <returns></returns>
        Task<TaxonTrendResult> CalculateTrendAsync(SearchFilter observationFilter, CheckListSearchFilter checkListSearchFilter);

        /// <summary>
        /// Get a area check list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CheckList> GetCheckListAsync(string id);
    }
}