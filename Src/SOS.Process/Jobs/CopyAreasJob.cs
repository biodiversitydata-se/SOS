using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class CopyAreasJob : ICopyAreasJob
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly IAreaProcessedRepository _areaProcessedRepository;
        private readonly ILogger<CopyAreasJob> _logger;

        public CopyAreasJob(
            IAreaVerbatimRepository areaVerbatimRepository,
            IAreaProcessedRepository areaProcessedRepository,
            ILogger<CopyAreasJob> logger)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _areaProcessedRepository = areaProcessedRepository ?? throw new ArgumentNullException(nameof(areaProcessedRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            var areas = await _areaVerbatimRepository.GetAllAsync();
            if (!areas?.Any() ?? true)
            {
                _logger.LogDebug("Failed to get areas");
                return false;
            }

            _logger.LogDebug("Start deleting areas");
            if (!await _areaProcessedRepository.DeleteCollectionAsync())
            {
                _logger.LogError("Failed to delete areas");
                return false;
            }
            _logger.LogDebug("Finish deleting areas");

            _logger.LogDebug("Start copy areas");
            var success = await _areaProcessedRepository.AddManyAsync(areas);
            //var success = await CopyAreas();
            _logger.LogDebug("Finish copy areas");

            return success ? true : throw new Exception("Copy field areas job failed");
        }
    }
}