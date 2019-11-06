using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using KulService;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Kul.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Repositories.Source.Kul
{
    public class KulSightingRepository : IKulSightingRepository
    {
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly ILogger<KulSightingRepository> _logger;

        public KulSightingRepository(ILogger<KulSightingRepository> logger, KulServiceConfiguration kulServiceConfiguration)
        {
            _logger = logger;
            _kulServiceConfiguration = kulServiceConfiguration;
        }

        public async Task<IEnumerable<KulService.WebSpeciesObservation>> GetAsync(DateTime changedFrom, DateTime changedTo)
        {
            KulService.SpeciesObservationChangeServiceClient client = new SpeciesObservationChangeServiceClient();
            var result = await client.GetSpeciesObservationChangeAsSpeciesAsync(
                _kulServiceConfiguration.Token,
                changedFrom,
                true,
                changedTo,
                true,
                0,
                false,
                _kulServiceConfiguration.MaxReturnedChanges);

            _logger.LogDebug($"Getting observations from KUL Service: ChangedFrom: {changedFrom.ToShortDateString()}, ChangedTo: {changedTo.ToShortDateString()}, Created: {result.CreatedSpeciesObservations.Length}, Updated: {result.UpdatedSpeciesObservations.Length}, Deleted: {result.DeletedSpeciesObservationGuids.Length}");

            return result.CreatedSpeciesObservations;
        }
    }
}
