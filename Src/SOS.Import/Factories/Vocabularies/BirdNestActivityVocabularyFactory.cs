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
    ///     Class for creating BirdNestActivity vocabulary.
    /// </summary>
    public class BirdNestActivityVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<ActivityVocabularyFactory> _logger;
        private const int BirdNestActivityIdMin = 1;
        private const int BirdNestActivityIdMax = 20;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public BirdNestActivityVocabularyFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<ActivityVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ??
                                             throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.BirdNestActivity;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var activities = await _artportalenMetadataRepository.GetActivitiesAsync();
            var birdNestActivities = activities.Where(m => m.Id >= BirdNestActivityIdMin && m.Id <= BirdNestActivityIdMax);
            var vocabularyValues = ConvertToVocabularyValuesWithCategory(birdNestActivities.ToArray());
            return vocabularyValues;
        }
    }
}