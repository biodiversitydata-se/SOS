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
    public class KulHarvestJob : IKulHarvestJob
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IKulObservationHarvester _kulObservationHarvester;
        private readonly ILogger<KulHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="kulObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public KulHarvestJob(
            IKulObservationHarvester kulObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<KulHarvestJob> logger)
        {
            _kulObservationHarvester = kulObservationHarvester ??
                                       throw new ArgumentNullException(nameof(kulObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest observations from KUL")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start KUL Harvest Job");
            var harvestInfoResult = await _kulObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End KUL Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            
            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("KUL Harvest Job failed");
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for KUL");
        }
    }
}