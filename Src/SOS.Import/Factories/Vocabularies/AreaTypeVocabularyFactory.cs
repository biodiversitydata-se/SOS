using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Area type vocabulary.
    /// </summary>
    public class AreaTypeVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<AreaTypeVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public AreaTypeVocabularyFactory(
            IMetadataRepository metadataRepository,
            ILogger<AreaTypeVocabularyFactory> logger)
        {
            _artportalenMetadataRepository =
                metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.AreaType;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var areaTypes = await _artportalenMetadataRepository.GetAreaTypesAsync();
            var vocabularyValues = base.ConvertToLocalizedVocabularyValues(areaTypes.ToArray());
            return vocabularyValues;
        }
    }
}