﻿using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Threading.Tasks;

namespace SOS.Lib.Managers
{
    /// <summary>
    /// Data quality manager
    /// </summary>
    public class DataQualityManager : IDataQualityManager
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly ILogger<DataQualityManager> _logger;

        public DataQualityManager(IProcessedObservationCoreRepository processedObservationRepository,
        ILogger<DataQualityManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<DataQualityReport> GetReportAsync(string organismGroup)
        {
            return await _processedObservationRepository.GetDataQualityReportAsync(organismGroup);
        }
    }
}