using System.Threading.Tasks;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos.Checklist;

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
        Task<double> CalculateTrendAsync(SearchFilter observationFilter, CheckListSearchFilter checkListSearchFilter);

        /// <summary>
        /// Get a check list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CheckListDto> GetCheckListAsync(string id);

        /// <summary>
        /// Get a internal check list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CheckListInternalDto> GetCheckListInternalAsync(string id);
    }
}