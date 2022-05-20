using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Search;
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
    public class CheckListManager : ICheckListManager
    {
        private readonly IProcessedCheckListRepository _processedCheckListRepository;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ILogger<CheckListManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedCheckListRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        public CheckListManager(
            IProcessedCheckListRepository processedCheckListRepository,
            IProcessedObservationRepository processedObservationRepository,
            ILogger<CheckListManager> logger)
        {
            _processedCheckListRepository = processedCheckListRepository ?? throw new ArgumentNullException(nameof(processedCheckListRepository));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedCheckListRepository.LiveMode = true;
            _processedObservationRepository.LiveMode = true;
        }

        /// <inheritdoc />
        public async Task<TaxonTrendResult> CalculateTrendAsync(SearchFilter observationFilter, CheckListSearchFilter checkListSearchFilter)
        {            
            // todo - use observationFilter in next version.

            var taxonTrendResult = new TaxonTrendResult();
            taxonTrendResult.NrPresentObservations = await _processedCheckListRepository.GetPresentCountAsync(checkListSearchFilter);
            taxonTrendResult.NrAbsentObservations = await _processedCheckListRepository.GetAbsentCountAsync(checkListSearchFilter);            
            taxonTrendResult.NrChecklists = await _processedCheckListRepository.GetChecklistCountAsync(checkListSearchFilter);
            taxonTrendResult.Quotient = taxonTrendResult.NrPresentObservations / (double)taxonTrendResult.NrChecklists;
            taxonTrendResult.TaxonId = checkListSearchFilter.Taxa.Ids.First();

            return taxonTrendResult;
        }

        /// <inheritdoc />
        public async Task<CheckListDto> GetCheckListAsync(string id)
        {
            try
            {
                var checkList = await _processedCheckListRepository.GetAsync(id, false);
                return checkList.ToDto();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get check list with id: {id}");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<CheckListInternalDto> GetCheckListInternalAsync(string id)
        {
            try
            {
                var checkList = await _processedCheckListRepository.GetAsync(id, true);
                return checkList.ToInternalDto();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get check list with id: {id}");
                return null;
            }
        }
    }
}