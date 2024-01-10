﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Enums;
using SOS.Harvest.Processors.Taxon.Interfaces;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Harvest.Processors.Taxon
{
    public class TaxonProcessor : ITaxonProcessor
    {
        private readonly ITaxonAttributeService _taxonAttributeService;
        private readonly ITaxonListService _taxonListService;
        private readonly ITaxonService _taxonService;
        private readonly ITaxonRepository _processedTaxonRepository;
        private readonly Repositories.Source.Artportalen.Interfaces.ITaxonRepository _apTaxonRepository;
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

            var taxaDictonary = taxa!.ToDictionary(t => t.Id, t => t);
            var currentRedlistPeriodId = await _taxonAttributeService.GetCurrentRedlistPeriodIdAsync();

            var taxonCount = taxa!.Count();
            var skip = 0;
            const int take = 500;

            var getTaxonAttributesTasks = new List<Task>();

            var allFactors = ((int[])Enum.GetValues(typeof(PeriodizeFactorEnum))).Concat((int[])Enum.GetValues(typeof(NonPeriodizeFactorEnum)));

            // Get taxon attribues to populate enumerations
            var taxonAttributes =
                       await _taxonAttributeService.GetTaxonAttributesAsync(new[] { 0 },
                           allFactors, Array.Empty<int>());

            if (!taxonAttributes?.Factors?.Any() ?? true)
            {
                throw new Exception("Failed to get taxon attribute types");
            }

            var attributeTypes = new Dictionary<int, IDictionary<string, string>>();

            foreach (var factor in taxonAttributes!.Factors)
            {
                if (!factor.AttributeGroup?.AttributeTypes?.Any() ?? true)
                {
                    continue;
                }
                foreach (var attributeType in factor.AttributeGroup!.AttributeTypes)
                {
                    if ((!attributeType.Enumerations?.Any() ?? true) || attributeTypes.ContainsKey(attributeType.AttributeTypeId))
                    {
                        continue;
                    }
                    var enumerations = new Dictionary<string, string>();
                    attributeTypes.Add(attributeType.AttributeTypeId, enumerations);

                    foreach (var enumeration in attributeType.Enumerations!)
                    {
                        if (string.IsNullOrEmpty(enumeration.Name))
                        {
                            continue;
                        }
                        enumerations.Add(enumeration.Value, enumeration.Name);
                    }
                }
            }

            while (skip < taxonCount)
            {
                var taxonIds = taxa!.Skip(skip).Take(take).Select(t => t.Id);

                await _semaphore.WaitAsync();
                getTaxonAttributesTasks.Add(PopulateDynamicProperties(taxaDictonary, attributeTypes, taxonIds));
                await _semaphore.WaitAsync();
                getTaxonAttributesTasks.Add(PopulateDynamicProperties(taxaDictonary, attributeTypes, taxonIds, currentRedlistPeriodId));
                skip += take;
            }
            await Task.WhenAll(getTaxonAttributesTasks);
        }

        /// <summary>
        ///     Populate dynamic properties from taxon lists
        /// </summary>
        /// <param name="taxa"></param>
        /// <returns></returns>
        private async Task AddTaxonListDataAsync(IEnumerable<DarwinCoreTaxon> taxa)
        {
            if (!taxa?.Any() ?? true)
            {
                return;
            }

            var taxaDictonary = taxa!.ToDictionary(t => t.Id, t => t);
            var lists = await _taxonListService.GetTaxaAsync(new[] { 34 /*Fridlysta arter*/, 281 /*Fridlysta fåglar*/ });
            
            foreach(var list in lists)
            {
                foreach(var taxonInfo in list.TaxonInformation)
                {
                    if(taxaDictonary.TryGetValue(taxonInfo.Id, out var taxon))
                    {
                        taxon.DynamicProperties.ProtectedByLaw = true;
                    }
                }
            }
        }

        /// <summary>
        /// Populate taxon attributes
        /// </summary>
        /// <param name="taxaDictonary"></param>
        /// <param name="attributeTypes"></param>
        /// <param name="taxonIds"></param>
        /// <param name="currentRedlistPeriodId"></param>
        /// <returns></returns>
        private async Task PopulateDynamicProperties(IDictionary<int, DarwinCoreTaxon> taxaDictonary,
            IDictionary<int, IDictionary<string, string>> attributeTypes,
            IEnumerable<int> taxonIds,
            int? currentRedlistPeriodId = null)
        {
            try
            {
                _logger.LogDebug("Start get taxon attributes batch");

                // Non periodized factors
                var factors = (int[])Enum.GetValues(typeof(NonPeriodizeFactorEnum));
                var periods = Array.Empty<int>();

                if (currentRedlistPeriodId.HasValue)
                {
                    // Periodized factors
                    factors = (int[])Enum.GetValues(typeof(PeriodizeFactorEnum));
                    periods = new[] { currentRedlistPeriodId ?? 0 };
                }

                var response =
                        await _taxonAttributeService.GetTaxonAttributesAsync(taxonIds,
                            factors,
                            currentRedlistPeriodId.HasValue ? new[] { currentRedlistPeriodId ?? 0 } : Array.Empty<int>());

                if (!response?.TaxonAttributes?.Any() ?? true)
                {
                    return;
                }

                foreach (var taxonAttribute in response!.TaxonAttributes)
                {
                    var mainField = taxonAttribute.Values?.FirstOrDefault(a => a.AttributeInfo?.IsMainField ?? false);

                    if (!taxaDictonary.TryGetValue(taxonAttribute.TaxonId, out var taxon) || mainField == null || taxonAttribute?.FactorId == null)
                    {
                        continue;
                    }

                    taxon.DynamicProperties ??= new TaxonDynamicProperties();
                    var enumValue = string.Empty;
                    if (attributeTypes.TryGetValue(mainField.AttributeInfo.AttributeTypeId, out var attributeType))
                    {
                        if (!string.IsNullOrEmpty(mainField.Value))
                        {
                            attributeType?.TryGetValue(mainField.Value, out enumValue);
                        }
                    };

                    if (currentRedlistPeriodId.HasValue)
                    {
                        switch ((PeriodizeFactorEnum)taxonAttribute.FactorId)
                        {
                            case PeriodizeFactorEnum.RedlistCategory:
                                taxon.DynamicProperties.RedlistCategory = mainField.Value;
                                break;
                        }
                    }
                    else
                    {
                        switch ((NonPeriodizeFactorEnum)taxonAttribute.FactorId)
                        {
                            case NonPeriodizeFactorEnum.ActionPlan:
                                taxon.DynamicProperties.ActionPlan =
                                    enumValue;
                                break;
                            case NonPeriodizeFactorEnum.BirdDirectiveAnnex1:
                            case NonPeriodizeFactorEnum.BirdDirectiveAnnex2:
                            case NonPeriodizeFactorEnum.PriorityBirds:
                                taxon.DynamicProperties.BirdDirective = mainField.Value == "1";
                                break;
                            case NonPeriodizeFactorEnum.DisturbanceRadius:
                                taxon.DynamicProperties.DisturbanceRadius = int.Parse(mainField.Value);
                                break;
                            case NonPeriodizeFactorEnum.EURegulation_1143_2014:
                                taxon.DynamicProperties.IsEURegulation_1143_2014 = mainField.Value == "1";
                                break;
                            case NonPeriodizeFactorEnum.Natura2000HabitatsDirectiveArticle2:
                                taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle2 = mainField.Value == "1";
                                break;
                            case NonPeriodizeFactorEnum.Natura2000HabitatsDirectiveArticle4:
                                taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle4 = mainField.Value == "1";
                                break;
                            case NonPeriodizeFactorEnum.Natura2000HabitatsDirectiveArticle5:
                                taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle5 = mainField.Value == "1";
                                break;
                            case NonPeriodizeFactorEnum.OrganismGroup:
                                taxon.DynamicProperties.OrganismGroup = enumValue;
                                break;
                            case NonPeriodizeFactorEnum.ProtectionLevel:
                                taxon.DynamicProperties.ProtectionLevel = enumValue;
                                break;
                            case NonPeriodizeFactorEnum.SwedishHistory:
                                taxon.DynamicProperties.SwedishHistoryId = mainField.Value;
                                taxon.DynamicProperties.SwedishHistory = enumValue;
                                break;
                            case NonPeriodizeFactorEnum.SwedishHistoryCategory:
                                taxon.DynamicProperties.SwedishHistoryCategory = enumValue;
                                break;
                            case NonPeriodizeFactorEnum.SwedishOccurrence:
                                taxon.DynamicProperties.SwedishOccurrence = enumValue;
                                break;
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
        /// <param name="taxonAttributeService"></param>
        /// <param name="taxonListService"></param>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="apTaxonRepository"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TaxonProcessor(
            ITaxonService taxonService,
            ITaxonAttributeService taxonAttributeService,
            ITaxonListService taxonListService,
            ITaxonRepository processedTaxonRepository,
            Repositories.Source.Artportalen.Interfaces.ITaxonRepository apTaxonRepository,
            ProcessConfiguration processConfiguration,
            ILogger<TaxonProcessor> logger)
        {

            _taxonService = taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _taxonAttributeService =
                taxonAttributeService ?? throw new ArgumentNullException(nameof(taxonAttributeService));
            _taxonListService = taxonListService ?? throw new ArgumentNullException(nameof(taxonListService));
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
                _logger.LogDebug("Start adding taxon attributes");
                await AddTaxonAttributesAsync(dwcTaxa);
                _logger.LogDebug("Finish adding taxon attributes");
                _logger.LogDebug("Start adding taxon list data");
                await AddTaxonListDataAsync(dwcTaxa);
                _logger.LogDebug("Finish adding taxon list data");
                var taxa = dwcTaxa.ToProcessedTaxa();

                var taxonCount = await _processedTaxonRepository.CountAllDocumentsAsync();
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
