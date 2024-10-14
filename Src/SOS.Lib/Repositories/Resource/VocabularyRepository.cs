using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Vocabulary repository.
    /// </summary>
    public class VocabularyRepository : RepositoryBase<Vocabulary, VocabularyId>, IVocabularyRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public VocabularyRepository(
            IProcessClient processClient,
            ILogger<VocabularyRepository> logger) : base(processClient, logger)
        {
        }
    }
}