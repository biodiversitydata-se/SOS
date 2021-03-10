using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Interfaces
{
    /// <summary>
    ///     Interface for vocabularies.
    /// </summary>
    public interface IVocabularyHarvester
    {
        /// <summary>
        ///     Import vocabularies to MongoDb.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestAsync();

        /// <summary>
        ///     Creates a vocabulary json file for specified field.
        /// </summary>
        /// <param name="vocabularyId"></param>
        /// <returns></returns>
        Task<(string Filename, byte[] Bytes)> CreateVocabularyFileAsync(VocabularyId vocabularyId);

        /// <summary>
        ///     Creates vocabulary for all specified fields and creates a zip file.
        /// </summary>
        /// <param name="vocabularyIds"></param>
        /// <returns></returns>
        Task<byte[]> CreateVocabulariesZipFileAsync(IEnumerable<VocabularyId> vocabularyIds);

        Task<IEnumerable<Vocabulary>> CreateAllVocabulariesAsync(
            IEnumerable<VocabularyId> vocabularyIds);
    }
}