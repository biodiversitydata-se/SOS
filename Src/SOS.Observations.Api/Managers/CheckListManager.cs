using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.CheckList;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
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
        public async Task<double> CalculateTrendAsync(SearchFilter observationFilter, CheckListSearchFilter checkListSearchFilter)
        {
            observationFilter.PositiveSightings = true;
            var positiveObservationsCount = await _processedObservationRepository.GetMatchCountAsync(observationFilter);

            observationFilter.PositiveSightings = false;
            var negativeObservationsCount = await _processedObservationRepository.GetMatchCountAsync(observationFilter);

            var negativeCheckListCount = await _processedCheckListRepository.GetTrendCountAsync(checkListSearchFilter);

            return ((double)positiveObservationsCount) / ((double)(positiveObservationsCount + negativeObservationsCount + negativeCheckListCount));
        }

        /// <inheritdoc />
        public async Task<CheckList> GetCheckListAsync(string id)
        {
            try
            {
                var checkList = await _processedCheckListRepository.GetAsync(id);
                return checkList;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get check list with id: {id}");
                return null;
            }
        }
    }
}