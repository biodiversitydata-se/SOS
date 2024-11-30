﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using System.Xml.Linq;

namespace SOS.Harvest.Services
{
    public class SersObservationService : ISersObservationService
    {
        private readonly IAquaSupportRequestService _aquaSupportRequestService;
        private readonly ILogger<SersObservationService> _logger;
        private readonly SersServiceConfiguration _sersServiceConfiguration;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="aquaSupportRequestService"></param>
        /// <param name="sersServiceConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SersObservationService(
            IAquaSupportRequestService aquaSupportRequestService,
            SersServiceConfiguration sersServiceConfiguration,
            ILogger<SersObservationService> logger)
        {
            _aquaSupportRequestService = aquaSupportRequestService ?? throw new ArgumentNullException(nameof(aquaSupportRequestService));
            _sersServiceConfiguration = sersServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(sersServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(DateTime startDate, DateTime endDate, long changeId)
        {
            try
            {
                return await _aquaSupportRequestService.GetAsync($"{_sersServiceConfiguration.BaseAddress}/api/v1/SersSpeciesObservation?token={_sersServiceConfiguration.Token}",
                    startDate, endDate, changeId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get data from {@dataProvider}", "SERS");
                throw;
            }
        }
    }
}