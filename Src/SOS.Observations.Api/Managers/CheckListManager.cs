using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Dtos.Checklist;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public class ChecklistManager : IChecklistManager
    {
        private readonly IProcessedChecklistRepository _processedChecklistRepository;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFilterManager _filterManager;
        private readonly ILogger<ChecklistManager> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="processedChecklistRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ChecklistManager(
            IProcessedChecklistRepository processedChecklistRepository,
            IProcessedObservationRepository processedObservationRepository,
            IFilterManager filterManager,
            ILogger<ChecklistManager> logger)
        {
            _processedChecklistRepository = processedChecklistRepository ?? throw new ArgumentNullException(nameof(processedChecklistRepository));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedChecklistRepository.LiveMode = true;
            _processedObservationRepository.LiveMode = true;
        }

        /// <inheritdoc />
        public async Task<TaxonTrendResult> CalculateTrendAsync(SearchFilter observationFilter, ChecklistSearchFilter checklistSearchFilter)
        {
            // todo - use observationFilter in next version.
            await _filterManager.PrepareFilterAsync(checklistSearchFilter);
            var taxonTrendResult = new TaxonTrendResult();
            taxonTrendResult.NrPresentObservations = await _processedChecklistRepository.GetPresentCountAsync(checklistSearchFilter);
            taxonTrendResult.NrAbsentObservations = await _processedChecklistRepository.GetAbsentCountAsync(checklistSearchFilter);            
            taxonTrendResult.NrChecklists = await _processedChecklistRepository.GetChecklistCountAsync(checklistSearchFilter);
            taxonTrendResult.Quotient = taxonTrendResult.NrPresentObservations / (double)taxonTrendResult.NrChecklists;
            taxonTrendResult.TaxonId = checklistSearchFilter.Taxa.Ids.First();

            return taxonTrendResult;
        }

        /// <inheritdoc />
        public async Task<ChecklistDto> GetChecklistAsync(string id)
        {
            try
            {
                var checklist = await _processedChecklistRepository.GetAsync(id, false);
                return checklist.ToDto();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get checklist with id: {id}");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ChecklistInternalDto> GetChecklistInternalAsync(string id)
        {
            try
            {
                var checklist = await _processedChecklistRepository.GetAsync(id, true);
                return checklist.ToInternalDto();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get checklist with id: {id}");
                return null;
            }
        }
    }
}