using Artportalen.Broadcast.Messages;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Hangfire.JobServer.ServiceBus.Consumers
{
    public class ArtportalenConsumer : IConsumer<SightingPublishedEvent>
    {
        private readonly IArtportalenObservationHarvester _artportalenObservationHarvester;
        private readonly IArtportalenObservationProcessor _artportalenObservationProcessor;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly ICache<int, Taxon> _taxonCache;
        private readonly ILogger<ArtportalenConsumer> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="artportalenObservationProcessor"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="taxonCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenConsumer(
            IArtportalenObservationHarvester artportalenObservationHarvester,
            IArtportalenObservationProcessor artportalenObservationProcessor,
            IDataProviderCache dataProviderCache,
            ICache<int, Taxon> taxonCache,
            ILogger<ArtportalenConsumer> logger)
        {
            _artportalenObservationHarvester = artportalenObservationHarvester ?? throw new ArgumentNullException(nameof(artportalenObservationHarvester));
            _artportalenObservationProcessor = artportalenObservationProcessor ?? throw new ArgumentNullException(nameof(artportalenObservationProcessor));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _taxonCache = taxonCache ?? throw new ArgumentNullException(nameof(taxonCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Consume message
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<SightingPublishedEvent> context)
        {
            var message = context.Message;
            try
            {
                _logger.LogInformation($"SightingPublishedEvent: Sighting id: {message.SightingId}. Taxon: {message.TaxonName} ({message.TaxonId}).");

                var verbatims = await _artportalenObservationHarvester.HarvestObservationsAsync(new[] { context.Message.SightingId }, JobCancellationToken.Null);

                if (!verbatims?.Any() ?? true)
                {
                    _logger.LogError($"Failed to harvest Sighting id: {message.SightingId}");
                    return;
                }

                var provider = await _dataProviderCache.GetAsync(1);
                var taxon = await _taxonCache.GetAsync(message.TaxonId);

                if (taxon == null)
                {
                    _logger.LogError($"Failed to find taxon with id: {message.TaxonId}");
                    return;
                }
                await _artportalenObservationProcessor.ProcessObservationsAsync(provider, new Dictionary<int, Taxon> { { taxon.Id, taxon } }, verbatims);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Failed to update Sighting id: {message.SightingId}");
            }
            
        }
    }
}
