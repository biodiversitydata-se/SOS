using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Vocabularies.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating County field mapping.
    /// </summary>
    public class CountyVocabularyFactory : GeoRegionVocabularyFactoryBase, IVocabularyFactory
    {
        public CountyVocabularyFactory(
            IAreaRepository _areaProcessedRepository,
            ILogger<CountyVocabularyFactory> logger) : base(_areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.Vocabulary> CreateVocabularyAsync()
        {
            return CreateVocabularyAsync(VocabularyId.County, AreaType.County);
        }
    }
}