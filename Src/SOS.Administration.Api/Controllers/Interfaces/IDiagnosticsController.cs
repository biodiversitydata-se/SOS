using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    /// Interface for field mapping controller
    /// </summary>
    public interface IDiagnosticsController
    {
        /// <summary>
        /// Get diff between generated, verbatim and processed field mappings.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetFieldMappingsDiffAsZipFile();

        /// <summary>
        /// Get diff between generated, verbatim and processed areas.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetAreasDiffAsZipFile();
    }
}