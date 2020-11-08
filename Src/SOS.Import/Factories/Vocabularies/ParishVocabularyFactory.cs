using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Vocabularies.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Parish field mapping.
    /// </summary>
    public class ParishVocabularyFactory : GeoRegionVocabularyFactoryBase, IVocabularyFactory
    {
        public ParishVocabularyFactory(
            IAreaRepository areaProcessedRepository,
            ILogger<ParishVocabularyFactory> logger) : base(areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.Vocabulary> CreateVocabularyAsync()
        {
            return CreateVocabularyAsync(VocabularyId.Parish, AreaType.Parish);
        }
    }
}