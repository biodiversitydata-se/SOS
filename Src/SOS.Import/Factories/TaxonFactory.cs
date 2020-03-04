using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Enums;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class TaxonFactory : Interfaces.ITaxonFactory { 

        private readonly ITaxonVerbatimRepository _taxonVerbatimRepository;
        private readonly ITaxonService _taxonService;
        private readonly ITaxonAttributeService _taxonAttributeService;
        private readonly ILogger<TaxonFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonVerbatimRepository"></param>
        /// <param name="taxonService"></param>
        /// <param name="taxonAttributeService"></param>
        /// <param name="logger"></param>
        public TaxonFactory(
            ITaxonVerbatimRepository taxonVerbatimRepository,
            ITaxonService taxonService,
            ITaxonAttributeService taxonAttributeService,
            ILogger<TaxonFactory> logger)
        {
            _taxonVerbatimRepository = taxonVerbatimRepository ?? throw new ArgumentNullException(nameof(taxonVerbatimRepository));
            _taxonService = taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _taxonAttributeService = taxonAttributeService ?? throw new ArgumentNullException(nameof(taxonAttributeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Aggregate clams
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestAsync()
        {
            var harvestInfo = new HarvestInfo(nameof(DarwinCoreTaxon), DataProvider.Artdatabanken, DateTime.Now);
            try
            {
                _logger.LogDebug("Start storing taxa verbatim");
                var taxa = await _taxonService.GetTaxaAsync();
                await AddTaxonAttributesAsync(taxa);
                await _taxonVerbatimRepository.DeleteCollectionAsync();
                await _taxonVerbatimRepository.AddCollectionAsync();
                await _taxonVerbatimRepository.AddManyAsync(taxa);
                
                _logger.LogDebug("Finish storing taxa verbatim");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = taxa?.Count() ?? 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest taxa");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        /// <summary>
        /// Populate dynamic properties from taxon attributes
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
            const int take = 1000;

            while (skip < taxonCount)
            {
                var taxonIds = taxa.Skip(skip).Take(take).Select(t => t.Id);

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
                                    taxon.DynamicProperties.ActionPlan = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.BirdDirective:
                                    taxon.DynamicProperties.BirdDirective = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value?.Contains("ja", StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.DisturbanceRadius:
                                    taxon.DynamicProperties.DisturbanceRadius = int.Parse(factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value ?? "0");
                                    break;
                                case FactorEnum.Natura2000HabitatsDirectiveArticle2:
                                    taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle2 = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value?.Contains("ja", StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.Natura2000HabitatsDirectiveArticle4:
                                    taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle4 = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value?.Contains("ja", StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.Natura2000HabitatsDirectiveArticle5:
                                    taxon.DynamicProperties.Natura2000HabitatsDirectiveArticle5 = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value.Contains("ja", StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.OrganismGroup:
                                    taxon.DynamicProperties.OrganismGroup = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.ProtectedByLaw:
                                    taxon.DynamicProperties.ProtectedByLaw = factor.Attributes?.FirstOrDefault(a => a.CompFieldIdx == 1)?.Value?.Contains("ja", StringComparison.CurrentCultureIgnoreCase);
                                    break;
                                case FactorEnum.ProtectionLevel:
                                    taxon.DynamicProperties.ProtectionLevel = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.RedlistCategory:
                                    taxon.DynamicProperties.RedlistCategory = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.SwedishHistory:
                                    taxon.DynamicProperties.SwedishHistory = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                                case FactorEnum.SwedishOccurrence:
                                    taxon.DynamicProperties.SwedishOccurrence = factor.Attributes?.FirstOrDefault(a => a.IsMainField)?.Value;
                                    break;
                            }
                        }
                    }
                }

                skip += take;
            }
        }
    }
}
