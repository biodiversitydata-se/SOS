using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonTree;
using SOS.Process.Services.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Area type vocabulary.
    /// </summary>
    public class TaxonCategoryVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly Repositories.Source.Artportalen.Interfaces.IMetadataRepository _artportalenMetadataRepository;        
        private readonly ITaxonService _taxonService;
        private readonly ILogger<TaxonCategoryVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public TaxonCategoryVocabularyFactory(
            Repositories.Source.Artportalen.Interfaces.IMetadataRepository metadataRepository,            
            ITaxonService taxonService,
            ILogger<TaxonCategoryVocabularyFactory> logger)
        {
            _artportalenMetadataRepository =
                metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));            
            _taxonService =
                taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.TaxonCategory;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {            
            var dwcTaxa = await _taxonService.GetTaxaAsync();
            var dwcTaxonById = dwcTaxa.ToDictionary(m => m.Id, m => m);
            var taxonCategories = TaxonCategoryHelper.GetTaxonCategories(dwcTaxonById);
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
    }
}