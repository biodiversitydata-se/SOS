using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating unit vocabulary.
    /// </summary>
    public class UnitVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<UnitVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public UnitVocabularyFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<UnitVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ??
                                             throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.Unit;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var validationStatusList = await _artportalenMetadataRepository.GetUnitsAsync();
            var vocabularyValues = base.ConvertToLocalizedVocabularyValues(validationStatusList.ToArray());
            return vocabularyValues;
        }
    }
}