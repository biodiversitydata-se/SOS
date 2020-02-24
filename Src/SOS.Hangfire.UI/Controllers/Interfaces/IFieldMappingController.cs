using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Hangfire.UI.Controllers.Interfaces
{
    /// <summary>
    /// Interface for field mapping controller
    /// </summary>
    public interface IFieldMappingController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> CreateAllFieldsMappingFilesAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldMappingFieldId"></param>
        /// <returns></returns>
        Task<IActionResult> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId);
    }
}
