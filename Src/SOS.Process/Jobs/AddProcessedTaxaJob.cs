using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Extensions;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class AddProcessedTaxaJob : IAddProcessedTaxaJob
    {
        private readonly ITaxonVerbatimRepository _taxonVerbatimRepository;
        private readonly ITaxonProcessedRepository _taxonProcessedRepository;
        private readonly ILogger<AddProcessedTaxaJob> _logger;

        public AddProcessedTaxaJob(
            ITaxonVerbatimRepository taxonVerbatimRepository,
            ITaxonProcessedRepository taxonProcessedRepository,
            ILogger<AddProcessedTaxaJob> logger)
        {
            _taxonVerbatimRepository = taxonVerbatimRepository ?? throw new ArgumentNullException(nameof(taxonVerbatimRepository));
            _taxonProcessedRepository = taxonProcessedRepository ?? throw new ArgumentNullException(nameof(taxonProcessedRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            var taxa = new List<ProcessedTaxon>();
            var skip = 0;
            var tmpTaxa = await _taxonVerbatimRepository.GetBatchAsync(skip);

            while (tmpTaxa?.Any() ?? false)
            {
                foreach (var taxon in tmpTaxa)
                {
                    taxa.Add(taxon.ToProcessedTaxon());
                }

                skip += tmpTaxa.Count();
                tmpTaxa = await _taxonVerbatimRepository.GetBatchAsync(skip);
            }

            if (!taxa?.Any() ?? true)
            {
                _logger.LogDebug("Failed to get taxa");
                return false;
            }

            _logger.LogDebug("Start deleting data from inactive instance");
            if (!await _taxonProcessedRepository.DeleteCollectionAsync())
            {
                _logger.LogError("Failed to delete ProcessedTaxon data");
                return false;
            }

            var result = await _taxonProcessedRepository.AddManyAsync(taxa);
            return result;
        }
    }
}
