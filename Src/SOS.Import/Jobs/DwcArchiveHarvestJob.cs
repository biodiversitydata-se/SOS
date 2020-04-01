using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    public class DwcArchiveHarvestJob : IDwcArchiveHarvestJob
    {
        private readonly IDwcObservationHarvester _dwcObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<DwcArchiveHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public DwcArchiveHarvestJob(
            IDwcObservationHarvester dwcObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<DwcArchiveHarvestJob> logger)
        {
            _dwcObservationHarvester = dwcObservationHarvester ?? throw new ArgumentNullException(nameof(dwcObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(string archivePath, IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start DwC-A Harvest Job");
            var result = await _dwcObservationHarvester.HarvestObservationsAsync(archivePath, cancellationToken);
            _logger.LogInformation($"End DwC-A Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("DwC-A Harvest Job failed");
        }
    }
}