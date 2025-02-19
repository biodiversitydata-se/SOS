﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating DeterminationMethod vocabulary.
    /// </summary>
    public class DeterminationMethodVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<DeterminationMethodVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public DeterminationMethodVocabularyFactory(IMetadataRepository artportalenMetadataRepository, ILogger<DeterminationMethodVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.DeterminationMethod;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>?> GetVocabularyValues()
        {
            var determinationMethods = await _artportalenMetadataRepository.GetDeterminationMethodsAsync();
            var vocabularyValues = ConvertToLocalizedVocabularyValues(determinationMethods.ToArray());
            return vocabularyValues;
        }
    }
}