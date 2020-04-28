using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvmService;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class MvmObservationService : Interfaces.IMvmObservationService
    {
        private readonly ISpeciesObservationChangeService _speciesObservationChangeServiceClient;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;
        private readonly ILogger<MvmObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesObservationChangeServiceClient"></param>
        /// <param name="mvmServiceConfiguration"></param>
        /// <param name="logger"></param>
        public MvmObservationService(
            ISpeciesObservationChangeService speciesObservationChangeServiceClient,
            MvmServiceConfiguration mvmServiceConfiguration,
            ILogger<MvmObservationService> logger)
        {
            _speciesObservationChangeServiceClient = speciesObservationChangeServiceClient ?? throw new ArgumentNullException(nameof(speciesObservationChangeServiceClient));
            _mvmServiceConfiguration = mvmServiceConfiguration ?? throw new ArgumentNullException(nameof(mvmServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<WebSpeciesObservation>> GetAsync(int getFromId)
        {
            var ready = await _speciesObservationChangeServiceClient.IsReadyToUseAsync(new IsReadyToUseRequest() { });                        
            var result = await _speciesObservationChangeServiceClient.GetSpeciesObservationChangeAsSpeciesAsync(new GetSpeciesObservationChangeAsSpeciesRequest() 
                { 
                    token = _mvmServiceConfiguration.Token,
                    changedFrom = DateTime.MinValue,
                    isChangedFromSpecified=false,
                    changedTo = DateTime.MaxValue,
                    isChangedToSpecified = false,
                    changeId = getFromId,
                    isChangedIdSpecified = true,
                    maxReturnedChanges = _mvmServiceConfiguration.MaxReturnedChangesInOnePage
                }
            );

            _logger.LogDebug($"Getting observations from MVM Service: From id: { getFromId }, Created: {result.GetSpeciesObservationChangeAsSpeciesResult?.CreatedSpeciesObservations?.Length ?? 0}, Updated: {result.GetSpeciesObservationChangeAsSpeciesResult?.UpdatedSpeciesObservations?.Length ?? 0}, Deleted: {result.GetSpeciesObservationChangeAsSpeciesResult?.DeletedSpeciesObservationGuids?.Length ?? 0}");
            
            return  result.GetSpeciesObservationChangeAsSpeciesResult.CreatedSpeciesObservations;            
        }
    }
}