﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Repositories.Destination.ClamPortal.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    /// <summary>
    ///     Clam Portal observation harvester
    /// </summary>
    public class ClamPortalObservationHarvester : IClamPortalObservationHarvester
    {
        private readonly IClamObservationService _clamObservationService;
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly ILogger<ClamPortalObservationHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="clamObservationService"></param>
        /// <param name="logger"></param>
        public ClamPortalObservationHarvester(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IClamObservationService clamObservationService,
            ILogger<ClamPortalObservationHarvester> logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(clamObservationVerbatimRepository));
            _clamObservationService =
                clamObservationService ?? throw new ArgumentNullException(nameof(clamObservationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Aggregate clams
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestClamsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(ClamObservationVerbatim), DataProviderType.ClamPortalObservations,
                DateTime.Now);
            try
            {
                _logger.LogDebug("Start storing clams verbatim");
                var items = await _clamObservationService.GetClamObservationsAsync();

                await _clamObservationVerbatimRepository.DeleteCollectionAsync();
                await _clamObservationVerbatimRepository.AddCollectionAsync();
                await _clamObservationVerbatimRepository.AddManyAsync(items);

                _logger.LogDebug("Finish storing clams verbatim");

                cancellationToken?.ThrowIfCancellationRequested();

                // Update harvest info
                harvestInfo.DataLastModified = items?.Select(o => o.Modified).Max();
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = items?.Count() ?? 0;
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, "Canceled harvest of clams");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of clams");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }
    }
}