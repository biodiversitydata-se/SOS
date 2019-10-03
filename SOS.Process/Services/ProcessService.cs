using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Services.Interfaces;


namespace SOS.Process.Services
{
    /// <summary>
    /// Main service
    /// </summary>
    public class ProcessService : Interfaces.IProcessService
    {
        private readonly IProcessedRepository _processRepository;

        private readonly ISpeciesPortalProcessFactory _speciesPortalProcessFactory;

        private readonly ITaxonService _taxonService;

        private readonly ILogger<ProcessService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processRepository"></param>
        /// <param name="speciesPortalProcessFactory"></param>
        /// <param name="logger"></param>
        public ProcessService(
            IProcessedRepository processRepository,
            ISpeciesPortalProcessFactory speciesPortalProcessFactory,
            ITaxonService taxonService,
            ILogger<ProcessService> logger)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            _speciesPortalProcessFactory = speciesPortalProcessFactory ?? throw new ArgumentNullException(nameof(speciesPortalProcessFactory));
            _taxonService = taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> ImportAsync(int sources)
        {
            _logger.LogDebug("Start getting taxa");
            var taxa = (await _taxonService.GetTaxaAsync())?.ToDictionary(t => t.TaxonID, t => t);

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
            if ((sources & 1) > 0)
            {
                processTasks.Add(_speciesPortalProcessFactory.ProcessAsync(taxa));
            }

            // Run all tasks async
            await Task.WhenAll(processTasks);

            var result = processTasks.All(t => t.Result);

            // Create index if great success
            if (result)
            {
                _logger.LogDebug("Create indexes");
                await _processRepository.CreateIndexAsync();
            }

            // return result of all imports
            return result;
        }
    }
}
