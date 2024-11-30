﻿using Microsoft.Extensions.Logging;
using MvmService;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using System.ServiceModel;

namespace SOS.Harvest.Services
{
    public class MvmObservationService : IMvmObservationService
    {
        private readonly ILogger<MvmObservationService> _logger;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;
        private readonly ISpeciesObservationChangeService _speciesObservationChangeServiceClient;

        private async Task<(long maxChangeId, WebSpeciesObservation[] observations)> GetAsync(long getFromId, byte attempt)
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
                _logger.LogDebug(
                    $"´Start getting observations from MVM Service: From id: {getFromId}");
                var result = response?.GetSpeciesObservationChangeAsSpeciesResult;
                _logger.LogDebug(
                    $"´Finish getting observations from MVM Service: From id: {getFromId}");

                return (result?.MaxChangeId ?? 0, result?.CreatedSpeciesObservations ?? new WebSpeciesObservation[0]);
            }
            catch (FaultException e)
            {
                _logger.LogError(e, "Failed to get {@dataProvider} observations from id: {@getFromId}, attempt: " + attempt, "MVM", getFromId);

                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get {@dataProvider} observations from id: {@getFromId}, attempt: " + attempt, "MVM", getFromId);
                // Give it up to tree attempts to get the data
                if (attempt < 3)
                {
                    Thread.Sleep(attempt * 1000);
                    return await GetAsync(getFromId, ++attempt);
                }

                throw;
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

        public async Task<(long MaxChangeId, WebSpeciesObservation[] Observations)> GetAsync(long getFromId)
        {
            return await GetAsync(getFromId, 1);
        }
    }
}