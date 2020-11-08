using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Interface for field mapping controller
    /// </summary>
    public interface IVocabulariesController
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> CreateAllFieldsMappingFilesAsync();

        /// <summary>
        /// </summary>
        /// <param name="fieldMappingFieldId"></param>
        /// <returns></returns>
        Task<IActionResult> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId);
    }
}