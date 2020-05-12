using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SersService;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class SersObservationService : Interfaces.ISersObservationService
    {
        private readonly ISpeciesObservationChangeService _speciesObservationChangeServiceClient;
        private readonly SersServiceConfiguration _sersServiceConfiguration;
        private readonly ILogger<SersObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sersServiceConfiguration"></param>
        public SersObservationService(
            ISpeciesObservationChangeService speciesObservationChangeServiceClient,
            SersServiceConfiguration sersServiceConfiguration,
            ILogger<SersObservationService> logger)
        {
            _speciesObservationChangeServiceClient = speciesObservationChangeServiceClient ?? throw new ArgumentNullException(nameof(speciesObservationChangeServiceClient));
            _sersServiceConfiguration = sersServiceConfiguration ?? throw new ArgumentNullException(nameof(sersServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Tuple<long, IEnumerable<WebSpeciesObservation>>> GetAsync(long getFromId)
        {
            var result = await _speciesObservationChangeServiceClient.GetSpeciesObservationChangeAsSpeciesAsync(
                _sersServiceConfiguration.Token,
                DateTime.MinValue, 
                false,
                DateTime.MaxValue, 
                false,
                getFromId,
                true,
                _sersServiceConfiguration.MaxReturnedChangesInOnePage);

            _logger.LogDebug($"Getting (max { _sersServiceConfiguration.MaxReturnedChangesInOnePage }) observations from SERS Service: From id: { getFromId }, Created: {result.CreatedSpeciesObservations?.Length ?? 0}, Updated: {result.UpdatedSpeciesObservations?.Length ?? 0}, Deleted: {result.DeletedSpeciesObservationGuids?.Length ?? 0}");

            return new Tuple<long, IEnumerable<WebSpeciesObservation>>(result.MaxChangeId, result.CreatedSpeciesObservations);
        }
    }
}
