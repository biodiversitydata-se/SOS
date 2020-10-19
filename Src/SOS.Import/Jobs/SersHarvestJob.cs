using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    public class SersHarvestJob : ISersHarvestJob
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<SersHarvestJob> _logger;
        private readonly ISersObservationHarvester _sersObservationHarvester;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sersObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public SersHarvestJob(
            ISersObservationHarvester sersObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<SersHarvestJob> logger)
        {
            _sersObservationHarvester = sersObservationHarvester ??
                                        throw new ArgumentNullException(nameof(sersObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest observations from SERS")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start SERS Harvest Job");
            var harvestInfoResult = await _sersObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End SERS Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("SERS Harvest Job failed");
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for SERS");
        }
    }
}