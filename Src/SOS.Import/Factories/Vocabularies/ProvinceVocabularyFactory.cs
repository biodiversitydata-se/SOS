using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Vocabularies.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Province field mapping.
    /// </summary>
    public class ProvinceVocabularyFactory : GeoRegionVocabularyFactoryBase, IVocabularyFactory
    {
        public ProvinceVocabularyFactory(
            IAreaRepository areaProcessedRepository,
            ILogger<ProvinceVocabularyFactory> logger) : base(areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.Vocabulary> CreateVocabularyAsync()
        {
            return CreateVocabularyAsync(VocabularyId.Province, AreaType.Province);
        }
    }
}