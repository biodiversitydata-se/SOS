using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Services
{
    public class FishDataObservationService : IFishDataObservationService
    {
        private readonly IAquaSupportRequestService _aquaSupportRequestService;
        private readonly FishDataServiceConfiguration _fishDataServiceConfiguration;
        private readonly ILogger<FishDataObservationService> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="aquaSupportRequestService"></param>
        /// <param name="fishDataServiceConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FishDataObservationService(
            IAquaSupportRequestService aquaSupportRequestService,
            FishDataServiceConfiguration fishDataServiceConfiguration,
            ILogger<FishDataObservationService> logger)
        {
            _aquaSupportRequestService = aquaSupportRequestService ?? throw new ArgumentNullException(nameof(aquaSupportRequestService));
            _fishDataServiceConfiguration = fishDataServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(fishDataServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(DateTime startDate, DateTime endDate, long changeId)
        {
            try
            {
                return await _aquaSupportRequestService.GetAsync($"{_fishDataServiceConfiguration.BaseAddress}/api/v1/FishDataSpeciesObservation?token={_fishDataServiceConfiguration.Token}",
                    startDate, endDate, changeId);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from Fish data", e);
                throw;
            }
        }
    }
}