using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Field mapping manager.
    /// </summary>
    public interface IVocabularyManager
    {
        /// <summary>
        ///     Get field mappings
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Vocabulary>> GetVocabulariesAsync();

        /// <summary>
        ///     Try get translated value.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="sosId"></param>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        bool TryGetTranslatedValue(VocabularyId fieldId, string cultureCode, int sosId,
            out string translatedValue);

        /// <summary>
        ///     Tries to get a non localized value.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="sosId"></param>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        bool TryGetValue(VocabularyId fieldId, int sosId, out string translatedValue);

        Task<byte[]> GetVocabulariesZipFileAsync(IEnumerable<VocabularyId> vocabularyIds);
    }
}