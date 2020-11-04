using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MvmService;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class MvmObservationService : IMvmObservationService
    {
        private readonly ILogger<MvmObservationService> _logger;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;
        private readonly ISpeciesObservationChangeService _speciesObservationChangeServiceClient;

        private async Task<Tuple<long, IEnumerable<WebSpeciesObservation>>> GetAsync(long getFromId, byte attempt)
        {
            try
            {
                var response = await _speciesObservationChangeServiceClient.GetSpeciesObservationChangeAsSpeciesAsync(
                    new GetSpeciesObservationChangeAsSpeciesRequest
                    {
                        token = _mvmServiceConfiguration.Token,
                        changedFrom = DateTime.MinValue,
                        isChangedFromSpecified = false,
                        changedTo = DateTime.MaxValue,
                        isChangedToSpecified = false,
                        changeId = getFromId,
                        isChangedIdSpecified = true,
                        maxReturnedChanges = _mvmServiceConfiguration.MaxReturnedChangesInOnePage
                    }
                );

                var result = response?.GetSpeciesObservationChangeAsSpeciesResult;

                _logger.LogDebug(
                    $"Getting observations from MVM Service: From id: {getFromId}, Created: {result?.CreatedSpeciesObservations?.Length ?? 0}, Updated: {result?.UpdatedSpeciesObservations?.Length ?? 0}, Deleted: {result?.DeletedSpeciesObservationGuids?.Length ?? 0}");
                return new Tuple<long, IEnumerable<WebSpeciesObservation>>(result.MaxChangeId,
                    result.CreatedSpeciesObservations);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get MVM observations from id: {getFromId}, attempt: {attempt}");
                // Give it up to tree attempts to get the data
                if (attempt < 4)
                {
                    return await GetAsync(getFromId, ++attempt);
                }

                return new Tuple<long, IEnumerable<WebSpeciesObservation>>(0L, null);
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="speciesObservationChangeServiceClient"></param>
        /// <param name="mvmServiceConfiguration"></param>
        /// <param name="logger"></param>
        public MvmObservationService(
            ISpeciesObservationChangeService speciesObservationChangeServiceClient,
            MvmServiceConfiguration mvmServiceConfiguration,
            ILogger<MvmObservationService> logger)
        {
            _speciesObservationChangeServiceClient = speciesObservationChangeServiceClient ??
                                                     throw new ArgumentNullException(
                                                         nameof(speciesObservationChangeServiceClient));
            _mvmServiceConfiguration = mvmServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(mvmServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Tuple<long, IEnumerable<WebSpeciesObservation>>> GetAsync(long getFromId)
        {
            return await GetAsync(getFromId, 1);
        }
    }
}