using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Area type vocabulary.
    /// </summary>
    public class TaxonCategoryVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly Repositories.Source.Artportalen.Interfaces.IMetadataRepository _artportalenMetadataRepository;
        private readonly ITaxonRepository _taxonRepository;
        private readonly ILogger<TaxonCategoryVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public TaxonCategoryVocabularyFactory(
            Repositories.Source.Artportalen.Interfaces.IMetadataRepository metadataRepository,
            ITaxonRepository taxonRepository,
            ILogger<TaxonCategoryVocabularyFactory> logger)
        {
            _artportalenMetadataRepository =
                metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _taxonRepository =
                taxonRepository ?? throw new ArgumentNullException(nameof(taxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.TaxonCategory;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var taxonTree = await GetTaxonTreeAsync();
            var taxonCategories = TaxonCategoryHelper.GetTaxonCategories(taxonTree);
            var vocabularyValues = GetVocabularyValues(taxonCategories);            
            return vocabularyValues;
        }

        private ICollection<VocabularyValueInfo> GetVocabularyValues(List<TaxonCategory> taxonCategories)
        {
            var vocabularyValues = new List<VocabularyValueInfo>();
            foreach (var taxonCategory in taxonCategories.OrderBy(m => m.Id))
            {
                var vocabularyValue = CreateVocabularyValue(taxonCategory);
                vocabularyValues.Add(vocabularyValue);
            }

            return vocabularyValues;
        }

        private VocabularyValueInfo CreateVocabularyValue(TaxonCategory taxonCategory)
        {
            var vocabularyValue = new VocabularyValueInfo();
            vocabularyValue.Id = taxonCategory.Id;
            vocabularyValue.Value = taxonCategory.EnglishName;
            vocabularyValue.Localized = true;
            vocabularyValue.Translations = new List<VocabularyValueTranslation>
            {
                new VocabularyValueTranslation
                {
                    CultureCode = Cultures.sv_SE, Value = taxonCategory.SwedishName
                },
                new VocabularyValueTranslation
                {
                    CultureCode = Cultures.en_GB, Value = taxonCategory.EnglishName
                }
            };

            return vocabularyValue;
        }

        private async Task<TaxonTree<IBasicTaxon>> GetTaxonTreeAsync()
        {
            var taxa = await _taxonRepository.GetAllAsync();
            var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);
            return taxonTree;
        }
    }
}