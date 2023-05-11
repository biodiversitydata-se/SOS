using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Vocabulary manager.
    /// </summary>
    public interface IVocabularyManager
    {
        /// <summary>
        ///     Get vocabularies.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Vocabulary>> GetVocabulariesAsync();

        /// <summary>
        ///     Try get translated value.
        /// </summary>
        /// <param name="vocabularyId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="sosId"></param>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        bool TryGetTranslatedValue(VocabularyId vocabularyId, string cultureCode, int sosId,
            out string translatedValue);

        /// <summary>
        ///     Tries to get a non localized value.
        /// </summary>
        /// <param name="vocabularyId"></param>
        /// <param name="sosId"></param>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        bool TryGetValue(VocabularyId vocabularyId, int sosId, out string translatedValue);

        Task<byte[]> GetVocabulariesZipFileAsync(IEnumerable<VocabularyId> vocabularyIds);
    }
}