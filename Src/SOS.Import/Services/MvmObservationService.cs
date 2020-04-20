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
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly ILogger<KulObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesObservationChangeServiceClient"></param>
        /// <param name="kulServiceConfiguration"></param>
        /// <param name="logger"></param>
        public MvmObservationService(
            ISpeciesObservationChangeService speciesObservationChangeServiceClient,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<KulObservationService> logger)
        {
            _speciesObservationChangeServiceClient = speciesObservationChangeServiceClient ?? throw new ArgumentNullException(nameof(speciesObservationChangeServiceClient));
            _kulServiceConfiguration = kulServiceConfiguration ?? throw new ArgumentNullException(nameof(kulServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<WebSpeciesObservation>> GetAsync(int getFromId)
        {
            /*SymmetricSecurityBindingElement
            var result = await _speciesObservationChangeServiceClient.GetSpeciesObservationChangeAsSpecies(
                _kulServiceConfiguration.Token,
                DateTime.MinValue,
                false,
                DateTime.MaxValue,
                false,
                getFromId,
                true,
                _kulServiceConfiguration.MaxReturnedChangesInOnePage);

            _logger.LogDebug($"Getting observations from KUL Service: From id: { getFromId }, Created: {result.CreatedSpeciesObservations?.Length ?? 0}, Updated: {result.UpdatedSpeciesObservations?.Length ?? 0}, Deleted: {result.DeletedSpeciesObservationGuids?.Length ?? 0}");
            
            return  result.CreatedSpeciesObservations;*/

            return null;
        }
    }
}