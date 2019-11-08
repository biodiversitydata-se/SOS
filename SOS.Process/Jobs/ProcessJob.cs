using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Services.Interfaces;

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

        private readonly ITaxonService _taxonService;

        private readonly ILogger<ProcessJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processRepository"></param>
        /// <param name="clamTreePortalProcessFactory"></param>
        /// <param name="kulProcessFactory"></param>
        /// <param name="speciesPortalProcessFactory"></param>
        /// <param name="taxonService"></param>
        /// <param name="logger"></param>
        public ProcessJob(
            IProcessedRepository processRepository,
            IClamTreePortalProcessFactory clamTreePortalProcessFactory,
            IKulProcessFactory kulProcessFactory,
            ISpeciesPortalProcessFactory speciesPortalProcessFactory,
            ITaxonService taxonService,
            ILogger<ProcessJob> logger)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            _kulProcessFactory = kulProcessFactory;
            _clamTreePortalProcessFactory = clamTreePortalProcessFactory ?? throw new ArgumentNullException(nameof(clamTreePortalProcessFactory));
            _speciesPortalProcessFactory = speciesPortalProcessFactory ?? throw new ArgumentNullException(nameof(speciesPortalProcessFactory));
            _taxonService = taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(int sources)
        {
            try
            {
                // Create task list
                _logger.LogDebug("Start getting taxa");

                // Map out taxon id
                var regex = new Regex(@"\d+$");
                var taxa = (await _taxonService.GetTaxaAsync())?.ToDictionary(t => regex.Match(t.TaxonID).Value, t => t);

                if (!taxa?.Any() ?? true)
                {
                    _logger.LogDebug("Failed to get taxa");

                    return false;
                }

                _logger.LogDebug("Empty collection");
                // Make sure we have an empty collection
                await _processRepository.DeleteCollectionAsync();
                await _processRepository.AddCollectionAsync();

                // Create task list
                var processTasks = new List<Task<bool>>();

                // Add species portal import if first bit is set
                if ((sources & (int)SightingProviders.SpeciesPortal) > 0)
                {
                    processTasks.Add(_speciesPortalProcessFactory.ProcessAsync(taxa));
                }

                if ((sources & (int)SightingProviders.ClamAndTreePortal) > 0)
                {
                    processTasks.Add(_clamTreePortalProcessFactory.ProcessAsync(taxa));
                }

                if ((sources & (int)SightingProviders.KUL) > 0)
                {
                    processTasks.Add(_kulProcessFactory.ProcessAsync(taxa));
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
            catch(Exception e)
            {
                _logger.LogError(e, "Process job failed");
                return false;
            }
            
        }
    }
}
