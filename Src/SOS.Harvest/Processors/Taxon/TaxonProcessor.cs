using Microsoft.Extensions.Logging;
using SOS.Harvest.Processors.Taxon.Interfaces;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Harvest.Processors.Taxon
{
    public class TaxonProcessor : ITaxonProcessor
    {
        private readonly ITaxonService _taxonService;
        private readonly ITaxonRepository _processedTaxonRepository;
        private readonly Repositories.Source.Artportalen.Interfaces.ITaxonRepository _apTaxonRepository;
        private readonly ILogger<TaxonProcessor> _logger;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        ///     Calculate higher classification tree by creating a taxon tree and iterate
        ///     each nodes parents.
        /// </summary>
        /// <param name="taxa"></param>
        private void CalculateHigherClassificationField(IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa,
            TaxonTree<IBasicTaxon> taxonTree)
        {
            if ((!taxa?.Any() ?? true) || taxonTree == null)
            {
                return;
            }

            foreach (var treeNode in taxonTree.TreeNodeById.Values)
            {
                var parentNames = treeNode.AsParentsNodeIterator().Select(m => m.ScientificName);
                var reversedParentNames = parentNames.Reverse();
                var higherClassification = string.Join(" | ", reversedParentNames);

                if (taxa!.TryGetValue(treeNode.TaxonId, out var taxon))
                {
                    taxon.HigherClassification = higherClassification;
                }
            }
        }

        private async Task PopulateSpeciesGroupField(IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa)
        {
            if (!taxa?.Any() ?? true)
            {
                return;
            }

            var apTaxa = await _apTaxonRepository.GetAsync();

            if (!apTaxa?.Any() ?? true)
            {
                return;
            }

            foreach (var aptaxon in apTaxa!)
            {
                if (taxa!.TryGetValue(aptaxon.Id, out var taxon))
                {
                    taxon.Attributes.SpeciesGroup = (SpeciesGroup)aptaxon.SpeciesGroupId!;
                }
            }
        }

        private bool ValidateTaxa(IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa)
        {
            var success = true;

            if (!taxa?.Any() ?? true)
            {
                _logger.LogInformation("No taxa to validate");
                return false;
            }
            var taxonCount = taxa!.Count();
            if (taxonCount < 105000)
            {
                _logger.LogInformation($"Only {taxonCount} taxon found, expect more than 105 000");
                return false;
            }

            foreach (var taxon in taxa!.Values)
            {
                if (string.IsNullOrEmpty(taxon.ScientificName))
                {
                    _logger.LogInformation($"Taxon: {taxon.Id} is missing a scientific name");
                    success = false;
                }
                /*   if (string.IsNullOrEmpty(taxon.TaxonRank))
                   {
                       _logger.LogInformation($"Taxon: {taxon.Id} is missing taxon rank");
                       success = false;
                   }
                   if (string.IsNullOrEmpty(taxon.TaxonomicStatus))
                   {
                       _logger.LogInformation($"Taxon: {taxon.Id} is missing taxonomic status");
                       success = false;
                   }*/
            }

            return success;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonService"></param>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="apTaxonRepository"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TaxonProcessor(
            ITaxonService taxonService,
            ITaxonRepository processedTaxonRepository,
            Repositories.Source.Artportalen.Interfaces.ITaxonRepository apTaxonRepository,
            ProcessConfiguration processConfiguration,
            ILogger<TaxonProcessor> logger)
        {
            _taxonService = taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _apTaxonRepository = apTaxonRepository ?? throw new ArgumentNullException(nameof(apTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (processConfiguration == null)
            {
                throw new ArgumentException(nameof(processConfiguration));
            }
            _semaphore = new SemaphoreSlim(processConfiguration.NoOfThreads, processConfiguration.NoOfThreads);
        }

        /// <inheritdoc />
        public async Task<int> ProcessTaxaAsync()
        {
            try
            {
                _logger.LogDebug("Start getting taxa");
                var dwcTaxa = await _taxonService.GetTaxaAsync();
                _logger.LogDebug("Finish getting taxa");
                var taxa = dwcTaxa.ToProcessedTaxa();

                var taxonCount = await _processedTaxonRepository.CountAllDocumentsAsync(estimateCount: false);
                var useCurrentCollection = false;
                if (!taxa?.Any() ?? true)
                {
                    _logger.LogWarning("No taxa harvested");
                    useCurrentCollection = true;
                }
                if (taxa!.Count() < taxonCount * 0.95)
                {
                    _logger.LogWarning("Less than 95% of previous number of taxa harvested");
                    useCurrentCollection = true;
                }

                var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);
                bool isTaxonDataOk = IsTaxonDataOk(taxa?.Values, taxonTree);
                if (!isTaxonDataOk)
                {
                    // If there are cycles in the data, use the current information in Taxon collection.
                    _logger.LogWarning("There are cycles in the taxa data");
                    useCurrentCollection = true;
                }

                // Check redlisted taxa
                int nrOfRedlistedTaxa = taxa!.Count(t => t.Value.Attributes.IsRedlisted);
                if (nrOfRedlistedTaxa < 1000)
                {
                    _logger.LogWarning($"Less than 1000 redlisted taxa found: {nrOfRedlistedTaxa}");
                    useCurrentCollection = true;
                }

                if (useCurrentCollection)
                {
                    _logger.LogInformation("Using current taxa collection");
                    return Convert.ToInt32(taxonCount);
                }

                _logger.LogDebug("Start calculating higher classification for taxa");
                CalculateHigherClassificationField(taxa, taxonTree);
                _logger.LogDebug("Finish calculating higher classification for taxa");

                _logger.LogDebug("Start populating species group for taxa");
                await PopulateSpeciesGroupField(taxa);
                _logger.LogDebug("Finish populating species group for taxa");

                _logger.LogDebug("Start validating taxa");
                if (!ValidateTaxa(taxa))
                {
                    _logger.LogError("Validation of taxa failed");
                    return -1;
                }
                _logger.LogDebug("Finish validating taxa");

                _logger.LogDebug("Start deleting processed taxa");
                if (!await _processedTaxonRepository.DeleteCollectionAsync())
                {
                    _logger.LogError("Failed to delete processed taxa");
                    return -1;
                }

                _logger.LogDebug("Finish deleting processed taxa");

                _logger.LogDebug("Start saving processed taxa");
                var success = await _processedTaxonRepository.AddManyAsync(taxa!.Values);
                await _processedTaxonRepository.WaitForDataInsert(taxa!.Values.Count, TimeSpan.FromMinutes(5));
                await Task.Delay(TimeSpan.FromSeconds(15)); // Extra wait security.
                _logger.LogDebug("Finish saving processed taxa");

                return success ? taxa.Count : -1;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process taxa");
            }

            return -1;
        }

        private bool IsTaxonDataOk(ICollection<Lib.Models.Processed.Observation.Taxon>? taxa,
            TaxonTree<IBasicTaxon> taxonTree)
        {
            var cycles = TaxonTreeCyclesDetectionHelper.CheckForCycles(taxonTree);
            if (cycles.Count > 0)
            {
                _logger.LogError(TaxonTreeCyclesDetectionHelper.GetCyclesDescription(cycles));
                return false;
            }

            return true;
        }
    }
}
