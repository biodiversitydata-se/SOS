using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Harvest.Enums;
using SOS.Harvest.Processors.Taxon.Interfaces;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Processors.Taxon
{
    public class TaxonProcessor : ITaxonProcessor
    {
        private readonly ITaxonAttributeService _taxonAttributeService;
        private readonly ITaxonService _taxonService;
        private readonly ITaxonRepository _processedTaxonRepository;
        private readonly ILogger<TaxonProcessor> _logger;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        ///     Populate dynamic properties from taxon attributes
        /// </summary>
        /// <param name="taxa"></param>
        /// <returns></returns>
        private async Task AddTaxonAttributesAsync(IEnumerable<DarwinCoreTaxon> taxa)
        {
            if (!taxa?.Any() ?? true)
            {
                return;
            }

            var taxaDictonary = taxa.ToDictionary(t => t.Id, t => t);
            var currentRedlistPeriodId = await _taxonAttributeService.GetCurrentRedlistPeriodIdAsync();

            var taxonCount = taxa.Count();
            var skip = 0;
            const int take = 500;

            var getTaxonAttributesTasks = new List<Task>();

            while (skip < taxonCount)
            {
                var taxonIds = taxa.Skip(skip).Take(take).Select(t => t.Id);

                await _semaphore.WaitAsync();

                getTaxonAttributesTasks.Add(GetTaxonAttributes(taxaDictonary, taxonIds, currentRedlistPeriodId));

                skip += take;
            }
            await Task.WhenAll(getTaxonAttributesTasks);
        }

        /// <summary>
        /// Get batch of taxon attributes
        /// </summary>
        /// <param name="taxaDictonary"></param>
        /// <param name="taxonIds"></param>
        /// <param name="currentRedlistPeriodId"></param>
        /// <returns></returns>
        private async Task GetTaxonAttributes(IDictionary<int, DarwinCoreTaxon> taxaDictonary, IEnumerable<int> taxonIds, int currentRedlistPeriodId)
        {
            try
            {
                _logger.LogDebug("Start get taxon attributes batch");
                var taxonAttributes =
                        await _taxonAttributeService.GetTaxonAttributesAsync(taxonIds,
                            (int[])Enum.GetValues(typeof(FactorEnum)),
                            new[] { 0, currentRedlistPeriodId });

                if (taxonAttributes?.Any() ?? false)
                {
                    foreach (var taxonAttribute in taxonAttributes)
                    {
                        if (!taxaDictonary.TryGetValue(taxonAttribute.TaxonId, out var taxon))
                        {
                            continue;
                        }

                        if (taxon.DynamicProperties == null)
                        {
                            taxon.DynamicProperties = new TaxonDynamicProperties();
                        }

                        foreach (var factor in taxonAttribute.Factors)
                        {
                            switch ((FactorEnum)factor.Id)
                            {
                                case FactorEnum.ActionPlan:
                                    taxon.DynamicProperties.ActionPlan =
                                        factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.BirdDirectiveAnnex1:
                                case FactorEnum.BirdDirectiveAnnex2:
                                case FactorEnum.PriorityBirds:
                                    taxon.DynamicProperties.BirdDirective = factor.Attributes
                                        ?.FirstOrDefault(a => a.IsMainField)?.Value?.Contains("ja",
                                            StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.DisturbanceRadius:
                                    taxon.DynamicProperties.DisturbanceRadius =
                                        int.Parse(factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value ?? "0");
                                    break;
                                case FactorEnum.Natura2000HabitatsDirectiveArticle2:
                                    taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle2 = factor.Attributes
                                        ?.FirstOrDefault(a => a.IsMainField)?.Value?.Contains("ja",
                                            StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.Natura2000HabitatsDirectiveArticle4:
                                    taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle4 = factor.Attributes
                                        ?.FirstOrDefault(a => a.IsMainField)?.Value?.Contains("ja",
                                            StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.Natura2000HabitatsDirectiveArticle5:
                                    taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle5 = factor.Attributes
                                        ?.FirstOrDefault(a => a.IsMainField)?.Value.Contains("ja",
                                            StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.OrganismGroup:
                                    taxon.DynamicProperties.OrganismGroup =
                                        factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.ProtectedByLawSpeciesProtection:
                                    taxon.DynamicProperties.ProtectedByLawSpeciesProtection = factor.Attributes
                                        ?.FirstOrDefault(a => a.CompFieldIdx == 1)?.Value?.Contains("ja",
                                            StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.ProtectedByLawBirds:
                                    taxon.DynamicProperties.ProtectedByLawBirds = factor.Attributes
                                        ?.FirstOrDefault(a => a.CompFieldIdx == 1)?.Value?.Contains("ja",
                                            StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.ProtectionLevel:
                                    taxon.DynamicProperties.ProtectionLevel =
                                        factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.RedlistCategory:
                                    taxon.DynamicProperties.RedlistCategory =
                                        factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.SwedishHistory:
                                    taxon.DynamicProperties.SwedishHistory =
                                        factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.SwedishOccurrence:
                                    taxon.DynamicProperties.SwedishOccurrence =
                                        factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                            }
                        }
                    }
                }
                _logger.LogDebug("Finish get taxon attributes batch");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        ///     Calculate higher classification tree by creating a taxon tree and iterate
        ///     each nodes parents.
        /// </summary>
        /// <param name="taxa"></param>
        private void CalculateHigherClassificationField(ICollection<Lib.Models.Processed.Observation.Taxon> taxa,
            TaxonTree<IBasicTaxon> taxonTree)
        {            
            var taxonById = taxa.ToDictionary(m => m.Id, m => m);
            foreach (var treeNode in taxonTree.TreeNodeById.Values)
            {
                var parentNames = treeNode.AsParentsNodeIterator().Select(m => m.ScientificName);
                var reversedParentNames = parentNames.Reverse();
                var higherClassification = string.Join(" | ", reversedParentNames);

                if (taxonById.TryGetValue(treeNode.TaxonId, out var taxon))
                {
                    taxon.HigherClassification = higherClassification;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taxonService"></param>
        /// <param name="taxonAttributeService"></param>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="logger"></param>
        public TaxonProcessor(
            ITaxonService taxonService,
            ITaxonAttributeService taxonAttributeService,
            ITaxonRepository processedTaxonRepository,
            ProcessConfiguration processConfiguration,
            ILogger<TaxonProcessor> logger)
        {
           
            _taxonService = taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _taxonAttributeService =
                taxonAttributeService ?? throw new ArgumentNullException(nameof(taxonAttributeService));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (processConfiguration == null)
            {
                throw new ArgumentException(nameof(processConfiguration));
            }
            _semaphore = new SemaphoreSlim(processConfiguration.NoOfThreads);
        }

        /// <inheritdoc />
        public async Task<int> ProcessTaxaAsync()
        {
            try
            {
                _logger.LogDebug("Getting taxa");
                var dwcTaxa = await _taxonService.GetTaxaAsync();
                _logger.LogDebug("Finish getting taxa");
                _logger.LogDebug("Adding taxon attributes");
                await AddTaxonAttributesAsync(dwcTaxa);
                _logger.LogDebug("Finish adding taxon attributes");

                var taxa = dwcTaxa.ToProcessedTaxa().ToList();
                var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);
                bool isTaxonDataOk = IsTaxonDataOk(taxa, taxonTree);
                // todo - don't replace taxonomy if data isn't ok.
                _logger.LogDebug("Start calculating higher classification for taxa");
                CalculateHigherClassificationField(taxa, taxonTree);
                _logger.LogDebug("Finish calculating higher classification for taxa");

                _logger.LogDebug("Start deleting processed taxa");
                if (!await _processedTaxonRepository.DeleteCollectionAsync())
                {
                    _logger.LogError("Failed to delete processed taxa");
                    return -1;
                }

                _logger.LogDebug("Finish deleting processed taxa");

                _logger.LogDebug("Start saving processed taxa");
                var success = await _processedTaxonRepository.AddManyAsync(taxa);
                _logger.LogDebug("Finish saving processed taxa");

                return success ? taxa?.Count ?? 0 : -1;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process taxa");
            }

            return -1;
        }

        private bool IsTaxonDataOk(ICollection<Lib.Models.Processed.Observation.Taxon> taxa,
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
