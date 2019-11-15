using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class ProcessJob : IProcessJob
    {
        private readonly IProcessedRepository _processRepository;

        private readonly ISpeciesPortalProcessFactory _speciesPortalProcessFactory;
        private readonly IClamTreePortalProcessFactory _clamTreePortalProcessFactory;
        private readonly IKulProcessFactory _kulProcessFactory;

        private readonly ITaxonVerbatimRepository _taxonVerbatimRepository;

        private readonly ILogger<ProcessJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processRepository"></param>
        /// <param name="clamTreePortalProcessFactory"></param>
        /// <param name="kulProcessFactory"></param>
        /// <param name="speciesPortalProcessFactory"></param>
        /// <param name="taxonVerbatimRepository"></param>
        /// <param name="logger"></param>
        public ProcessJob(
            IProcessedRepository processRepository,
            IClamTreePortalProcessFactory clamTreePortalProcessFactory,
            IKulProcessFactory kulProcessFactory,
            ISpeciesPortalProcessFactory speciesPortalProcessFactory,
            ITaxonVerbatimRepository taxonVerbatimRepository,
            ILogger<ProcessJob> logger)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            _kulProcessFactory = kulProcessFactory;
            _clamTreePortalProcessFactory = clamTreePortalProcessFactory ?? throw new ArgumentNullException(nameof(clamTreePortalProcessFactory));
            _speciesPortalProcessFactory = speciesPortalProcessFactory ?? throw new ArgumentNullException(nameof(speciesPortalProcessFactory));
            _taxonVerbatimRepository = taxonVerbatimRepository ?? throw new ArgumentNullException(nameof(taxonVerbatimRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(int sources, IJobCancellationToken cancellationToken)
        {
            try
            {
                // Create task list
                _logger.LogDebug("Start getting taxa");

                // Map out taxon id
                var taxa = new Dictionary<int, DarwinCoreTaxon>();
                var skip = 0;
                var tmpTaxa = await _taxonVerbatimRepository.GetBatchAsync(skip);

                while (tmpTaxa?.Any() ?? false)
                {
                    foreach (var taxon in tmpTaxa)
                    {
                        taxa.Add(taxon.Id, taxon);
                    }

                    skip += tmpTaxa.Count();
                    tmpTaxa = await _taxonVerbatimRepository.GetBatchAsync(skip);
                }

                if (!taxa?.Any() ?? true)
                {
                    _logger.LogDebug("Failed to get taxa");

                    return false;
                }

                cancellationToken?.ThrowIfCancellationRequested();
                _logger.LogDebug("Empty collection");
                // Make sure we have an empty collection
                await _processRepository.DeleteCollectionAsync();
                await _processRepository.AddCollectionAsync();
                cancellationToken?.ThrowIfCancellationRequested();

                // Create task list
                var processTasks = new List<Task<bool>>();

                // Add species portal import if first bit is set
                if ((sources & (int)SightingProviders.SpeciesPortal) > 0)
                {
                    processTasks.Add(_speciesPortalProcessFactory.ProcessAsync(taxa, cancellationToken));
                }

                if ((sources & (int)SightingProviders.ClamAndTreePortal) > 0)
                {
                    processTasks.Add(_clamTreePortalProcessFactory.ProcessAsync(taxa, cancellationToken));
                }

                if ((sources & (int)SightingProviders.KUL) > 0)
                {
                    processTasks.Add(_kulProcessFactory.ProcessAsync(taxa, cancellationToken));
                }

                // Run all tasks async
                await Task.WhenAll(processTasks);

                var success = processTasks.All(t => t.Result);

                // Create index if great success
                if (success)
                {
                    _logger.LogDebug("Create indexes");
                    await _processRepository.CreateIndexAsync();
                }

                // return result of all processing
                return success;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Process job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Process job failed");
                return false;
            }            
        }
    }
}
