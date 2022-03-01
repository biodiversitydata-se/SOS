using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Services
{
    public class KulObservationService : IKulObservationService
    {
        private readonly IAquaSupportRequestService _aquaSupportRequestService;
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly ILogger<KulObservationService> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="aquaSupportRequestService"></param>
        /// <param name="kulServiceConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public KulObservationService(
            IAquaSupportRequestService aquaSupportRequestService,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<KulObservationService> logger)
        {
            _aquaSupportRequestService = aquaSupportRequestService ?? throw new ArgumentNullException(nameof(aquaSupportRequestService));
            _kulServiceConfiguration = kulServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(kulServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(DateTime startDate, DateTime endDate, long changeId)
        {
            try
            {
                return await _aquaSupportRequestService.GetAsync($"{_kulServiceConfiguration.BaseAddress}/api/v1/KulSpeciesObservation?token={_kulServiceConfiguration.Token}",
                    startDate, endDate, changeId);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from KUL", e);
                throw;
            }
        }
    }
}