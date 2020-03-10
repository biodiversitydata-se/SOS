using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Process.Extensions;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ClamPortalProcessFactory : DataProviderProcessorBase<ClamPortalProcessFactory>, Interfaces.IClamPortalProcessFactory
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        public override DataProvider DataProvider => DataProvider.ClamPortal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="logger"></param>
        public ClamPortalProcessFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedSightingRepository processedSightingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<ClamPortalProcessFactory> logger) : base(processedSightingRepository, fieldMappingResolverHelper, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        protected override async Task<int> ProcessObservations(IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedSighting> sightings = new List<ProcessedSighting>();

            // Process and commit in batches.
            using var cursor = await _clamObservationVerbatimRepository.GetAllAsync();
            await cursor.ForEachAsync(c =>
            {
                ProcessedSighting processedSighting = c.ToProcessed(taxa);
                _areaHelper.AddAreaDataToProcessedSighting(processedSighting);
                sightings.Add(processedSighting);
                if (IsBatchFilledToLimit(sightings.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += CommitBatch(sightings);
                    Logger.LogDebug($"Clam Portal Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (sightings.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += CommitBatch(sightings);
                Logger.LogDebug($"Clam Portal Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}