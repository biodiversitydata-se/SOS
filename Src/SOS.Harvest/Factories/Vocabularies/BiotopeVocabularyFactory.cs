﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating biotope vocabulary.
    /// </summary>
    public class BiotopeVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<BiotopeVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public BiotopeVocabularyFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<BiotopeVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ??
                                             throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.Biotope;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>?> GetVocabularyValues()
        {
            var biotopes = await _artportalenMetadataRepository.GetBiotopesAsync();
            var vocabularyValues = ConvertToLocalizedVocabularyValues(biotopes.ToArray());
            return vocabularyValues;
        }
    }
}