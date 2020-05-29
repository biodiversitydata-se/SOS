using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KulService;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class KulObservationService : IKulObservationService
    {
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly ILogger<KulObservationService> _logger;
        private readonly ISpeciesObservationChangeService _speciesObservationChangeServiceClient;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="speciesObservationChangeServiceClient"></param>
        /// <param name="kulServiceConfiguration"></param>
        /// <param name="logger"></param>
        public KulObservationService(
            ISpeciesObservationChangeService speciesObservationChangeServiceClient,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<KulObservationService> logger)
        {
            _speciesObservationChangeServiceClient = speciesObservationChangeServiceClient ??
                                                     throw new ArgumentNullException(
                                                         nameof(speciesObservationChangeServiceClient));
            _kulServiceConfiguration = kulServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(kulServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<WebSpeciesObservation>> GetAsync(DateTime changedFrom, DateTime changedTo)
        {
            var result = await _speciesObservationChangeServiceClient.GetSpeciesObservationChangeAsSpeciesAsync(
                _kulServiceConfiguration.Token,
                changedFrom,
                true,
                changedTo,
                true,
                0,
                false,
                _kulServiceConfiguration.MaxReturnedChangesInOnePage);

            _logger.LogDebug(
                $"Getting observations from KUL Service: ChangedFrom: {changedFrom.ToShortDateString()}, ChangedTo: {changedTo.ToShortDateString()}, Created: {result.CreatedSpeciesObservations?.Length ?? 0}, Updated: {result.UpdatedSpeciesObservations?.Length ?? 0}, Deleted: {result.DeletedSpeciesObservationGuids?.Length ?? 0}");

            return result.CreatedSpeciesObservations;
        }
    }
}