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
            /*SymmetricSecurityBindingElement
            var result = await _speciesObservationChangeServiceClient.GetSpeciesObservationChangeAsSpecies(
                _mvmServiceConfiguration.Token,
                DateTime.MinValue,
                false,
                DateTime.MaxValue,
                false,
                getFromId,
                true,
                _mvmServiceConfiguration.MaxReturnedChangesInOnePage);

            _logger.LogDebug($"Getting observations from MVM Service: From id: { getFromId }, Created: {result.CreatedSpeciesObservations?.Length ?? 0}, Updated: {result.UpdatedSpeciesObservations?.Length ?? 0}, Deleted: {result.DeletedSpeciesObservationGuids?.Length ?? 0}");
            
            return  result.CreatedSpeciesObservations;*/

            return null;
        }
    }
}