using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Vocabularies.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Municipality field mapping.
    /// </summary>
    public class MunicipalityVocabularyFactory : GeoRegionVocabularyFactoryBase, IVocabularyFactory
    {
        public MunicipalityVocabularyFactory(
            IAreaRepository areaProcessedRepository,
            ILogger<MunicipalityVocabularyFactory> logger) : base(areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.Vocabulary> CreateVocabularyAsync()
        {
            return CreateVocabularyAsync(VocabularyId.Municipality, AreaType.Municipality);
        }
    }
}