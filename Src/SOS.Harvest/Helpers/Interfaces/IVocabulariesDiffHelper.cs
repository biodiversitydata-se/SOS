using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Helpers.Interfaces
{
    public interface IVocabulariesDiffHelper
    {
        /// <summary>
        ///     Checks for differences between generated, verbatim and processed vocabularies
        ///     and returns the result in a zip file.
        /// </summary>
        /// <param name="generatedVocabularies"></param>
        /// <returns></returns>
        Task<byte[]> CreateDiffZipFile(IEnumerable<Vocabulary> generatedVocabularies);
    }
}