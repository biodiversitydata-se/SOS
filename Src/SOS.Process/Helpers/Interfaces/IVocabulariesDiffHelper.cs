using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Helpers.Interfaces
{
    public interface IVocabulariesDiffHelper
    {
        /// <summary>
        ///     Checks for differences between generated, verbatim and processed field mappings
        ///     and returns the result in a zip file.
        /// </summary>
        /// <param name="generatedVocabularies"></param>
        /// <returns></returns>
        Task<byte[]> CreateDiffZipFile(IEnumerable<Vocabulary> generatedVocabularies);
    }
}