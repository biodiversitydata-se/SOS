﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating DiscoveryMethod vocabulary.
    /// </summary>
    public class DiscoveryMethodVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<DiscoveryMethodVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public DiscoveryMethodVocabularyFactory(IMetadataRepository artportalenMetadataRepository, ILogger<DiscoveryMethodVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.DiscoveryMethod;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>?> GetVocabularyValues()
        {
            var discoveryMethods = await _artportalenMetadataRepository.GetDiscoveryMethodsAsync();
            var vocabularyValues = ConvertToLocalizedVocabularyValues(discoveryMethods.ToArray());
            return vocabularyValues;
        }
    }
}