using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Interface for vocabularies controller
    /// </summary>
    public interface IVocabulariesController
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> CreateAllVocabulariesFilesAsync();

        /// <summary>
        /// </summary>
        /// <param name="vocabularyId"></param>
        /// <returns></returns>
        Task<IActionResult> CreateSingleVocabularyFileAsync(VocabularyId vocabularyId);

        /// <summary>
        ///     Run import vocabularies.
        /// </summary>
        /// <returns></returns>
        IActionResult RunImportVocabulariesJob();
    }
}