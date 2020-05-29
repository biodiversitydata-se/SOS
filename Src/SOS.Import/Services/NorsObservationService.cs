using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NorsService;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class NorsObservationService : INorsObservationService
    {
        private readonly ILogger<NorsObservationService> _logger;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;
        private readonly ISpeciesObservationChangeService _speciesObservationChangeServiceClient;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="norsServiceConfiguration"></param>
        public NorsObservationService(
            ISpeciesObservationChangeService speciesObservationChangeServiceClient,
            NorsServiceConfiguration norsServiceConfiguration,
            ILogger<NorsObservationService> logger)
        {
            _speciesObservationChangeServiceClient = speciesObservationChangeServiceClient ??
                                                     throw new ArgumentNullException(
                                                         nameof(speciesObservationChangeServiceClient));
            _norsServiceConfiguration = norsServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(norsServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Tuple<long, IEnumerable<WebSpeciesObservation>>> GetAsync(long getFromId)
        {
            var result = await _speciesObservationChangeServiceClient.GetSpeciesObservationChangeAsSpeciesAsync(
                _norsServiceConfiguration.Token,
                DateTime.MinValue,
                false,
                DateTime.MaxValue,
                false,
                getFromId,
                true,
                _norsServiceConfiguration.MaxReturnedChangesInOnePage);

            _logger.LogDebug(
                $"Getting (max {_norsServiceConfiguration.MaxReturnedChangesInOnePage}) observations from NORS Service: From id: {getFromId}, Created: {result?.CreatedSpeciesObservations?.Length ?? 0}, Updated: {result?.UpdatedSpeciesObservations?.Length ?? 0}, Deleted: {result?.DeletedSpeciesObservationGuids?.Length ?? 0}");

            return new Tuple<long, IEnumerable<WebSpeciesObservation>>(result.MaxChangeId,
                result.CreatedSpeciesObservations);
        }
    }
}