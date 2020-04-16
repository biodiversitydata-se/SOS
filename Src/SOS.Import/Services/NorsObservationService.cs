using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NorsService;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class NorsObservationService : Interfaces.INorsObservationService
    {
        private readonly NorsServiceConfiguration _norsServiceConfiguration;
        private readonly ILogger<NorsObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="norsServiceConfiguration"></param>
        public NorsObservationService(ILogger<NorsObservationService> logger, NorsServiceConfiguration norsServiceConfiguration)
        {
            _logger = logger;
            _norsServiceConfiguration = norsServiceConfiguration;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<NorsService.WebSpeciesObservation>> GetAsync(int getFromId)
        {
            var client = new SpeciesObservationChangeServiceClient();
            var result = await client.GetSpeciesObservationChangeAsSpeciesAsync(
                _norsServiceConfiguration.Token,
                DateTime.MinValue, 
                false,
                DateTime.MaxValue,
                false,
                getFromId,
                true,
                _norsServiceConfiguration.MaxReturnedChangesInOnePage);

            _logger.LogDebug($"Getting (max { _norsServiceConfiguration.MaxReturnedChangesInOnePage }) observations from NORS Service: From id: {getFromId}, Created: {result?.CreatedSpeciesObservations?.Length}, Updated: {result?.UpdatedSpeciesObservations?.Length}, Deleted: {result?.DeletedSpeciesObservationGuids?.Length}");

            return result.CreatedSpeciesObservations;
        }
    }
}
