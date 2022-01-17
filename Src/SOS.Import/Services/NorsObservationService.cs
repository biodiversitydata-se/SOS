using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class NorsObservationService : INorsObservationService
    {
        private readonly IAquaSupportRequestService _aquaSupportRequestService;
        private readonly ILogger<NorsObservationService> _logger;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="aquaSupportRequestService"></param>
        /// <param name="norsServiceConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NorsObservationService(
            IAquaSupportRequestService aquaSupportRequestService,
            NorsServiceConfiguration norsServiceConfiguration,
            ILogger<NorsObservationService> logger)
        {
            _aquaSupportRequestService = aquaSupportRequestService ?? throw new ArgumentNullException(nameof(aquaSupportRequestService));
            _norsServiceConfiguration = norsServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(norsServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(DateTime startDate, DateTime endDate, long changeId)
        {
            try
            {
                return await _aquaSupportRequestService.GetAsync($"{_norsServiceConfiguration.BaseAddress}/api/v1/NorsSpeciesObservation?token={_norsServiceConfiguration.Token}",
                    startDate, endDate, changeId);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from NORS", e);
                throw;
            }
        }
    }
}