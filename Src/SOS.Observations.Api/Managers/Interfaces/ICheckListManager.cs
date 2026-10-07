using SOS.Api.Contracts.Checklists.Responses;
using SOS.Lib.Models.Search.Filters;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers.Interfaces;

/// <summary>
///     Area manager
/// </summary>
public interface IChecklistManager
{
    /// <summary>
    /// Calculate taxon trend
    /// </summary>
    /// <param name="observationFilter"></param>
    /// <param name="checklistSearchFilter"></param>
    /// <returns></returns>
    Task<TaxonTrendResult> CalculateTrendAsync(SearchFilter observationFilter, ChecklistSearchFilter checklistSearchFilter);

    /// <summary>
    /// Get a checklist
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ChecklistDto> GetChecklistAsync(string id);

    /// <summary>
    /// Get a internal checklist
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ChecklistInternalDto> GetChecklistInternalAsync(string id);
}
