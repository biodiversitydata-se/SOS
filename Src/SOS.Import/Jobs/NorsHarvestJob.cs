using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Jobs
{
    public class NorsHarvestJob : INorsHarvestJob
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<NorsHarvestJob> _logger;
        private readonly INorsObservationHarvester _norsObservationHarvester;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="norsObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public NorsHarvestJob(
            INorsObservationHarvester norsObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<NorsHarvestJob> logger)
        {
            _norsObservationHarvester = norsObservationHarvester ??
                                        throw new ArgumentNullException(nameof(norsObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest observations from NORS")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start NORS Harvest Job");
            var harvestInfoResult = await _norsObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End NORS Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            
            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("NORS Harvest Job failed");
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for NORS");
        }
    }
}