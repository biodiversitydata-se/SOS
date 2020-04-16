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
        private readonly SersServiceConfiguration _sersServiceConfiguration;
        private readonly ILogger<SersObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sersServiceConfiguration"></param>
        public SersObservationService(ILogger<SersObservationService> logger, SersServiceConfiguration sersServiceConfiguration)
        {
            _logger = logger;
            _sersServiceConfiguration = sersServiceConfiguration;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SersService.WebSpeciesObservation>> GetAsync(int getFromId)
        {
            var client = new SpeciesObservationChangeServiceClient();
            var result = await client.GetSpeciesObservationChangeAsSpeciesAsync(
                _sersServiceConfiguration.Token,
                DateTime.MinValue, 
                false,
                DateTime.MaxValue, 
                false,
                getFromId,
                true,
                _sersServiceConfiguration.MaxReturnedChangesInOnePage);

            _logger.LogDebug($"Getting (max { _sersServiceConfiguration.MaxReturnedChangesInOnePage }) observations from SERS Service: From id: { getFromId }, Created: {result.CreatedSpeciesObservations.Length}, Updated: {result.UpdatedSpeciesObservations.Length}, Deleted: {result.DeletedSpeciesObservationGuids.Length}");

            return result.CreatedSpeciesObservations;
        }
    }
}
