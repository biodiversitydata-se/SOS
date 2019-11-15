using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;

namespace SOS.Import.Jobs
{
    public class KulHarvestJob : IKulHarvestJob
    {
        private readonly IKulObservationFactory _kulObservationFactory;
        private readonly ILogger<KulHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationFactory"></param>
        /// <param name="logger"></param>
        public KulHarvestJob(IKulObservationFactory kulObservationFactory, ILogger<KulHarvestJob> logger)
        {
            _kulObservationFactory = kulObservationFactory ?? throw new ArgumentNullException(nameof(kulObservationFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(IJobCancellationToken  cancellationToken)
        {
            _logger.LogDebug("Start KUL Harvest Job");
            var success = await _kulObservationFactory.HarvestObservationsAsync(cancellationToken);
            _logger.LogDebug($"End KUL Harvest Job. Success: {success}");
            return success;
        }
    }
}