using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class ProcessTaxaJob : IProcessTaxaJob
    {
        private readonly ITaxonVerbatimRepository _taxonVerbatimRepository;
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly ILogger<ProcessTaxaJob> _logger;

        public ProcessTaxaJob(
            ITaxonVerbatimRepository taxonVerbatimRepository,
            IProcessedTaxonRepository processedTaxonRepository,
            ILogger<ProcessTaxaJob> logger)
        {
            _taxonVerbatimRepository = taxonVerbatimRepository ?? throw new ArgumentNullException(nameof(taxonVerbatimRepository));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            var dwcTaxa = await _taxonVerbatimRepository.GetAllAsync();
            if (!dwcTaxa?.Any() ?? true)
            {
                _logger.LogDebug("Failed to get taxa");
                return false;
            }

            // Process taxa
            var taxa = dwcTaxa.Select(m => m.ToProcessedTaxon()).ToList();
            CalculateHigherClassificationField(taxa);

            _logger.LogDebug("Start deleting processed taxa");
            if (!await _processedTaxonRepository.DeleteCollectionAsync())
            {
                _logger.LogError("Failed to delete processed taxa");
                return false;
            }
            _logger.LogDebug("Finish deleting processed taxa");

            _logger.LogDebug("Start copy processed taxa");
            var success = await _processedTaxonRepository.AddManyAsync(taxa);
            _logger.LogDebug("Finish copy processed taxa");

            return success ? true : throw new Exception("Process taxa job failed");
        }

        /// <summary>
        /// Calculate higher classification tree by creating a taxon tree and iterate
        /// each nodes parents.
        /// </summary>
        /// <param name="taxa"></param>
        private void CalculateHigherClassificationField(ICollection<ProcessedTaxon> taxa)
        {
            var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);
            var taxonById = taxa.ToDictionary(m => m.Id, m => m);
            foreach (var treeNode in taxonTree.TreeNodeById.Values)
            {
                var parentNames = treeNode.AsParentsNodeIterator().Select(m => m.ScientificName);
                var reversedParentNames = parentNames.Reverse();
                string higherClassification = string.Join(" | ", reversedParentNames);
                taxonById[treeNode.TaxonId].HigherClassification = higherClassification;
            }
        }
    }
}