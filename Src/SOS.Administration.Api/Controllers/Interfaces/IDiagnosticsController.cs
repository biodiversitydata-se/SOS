using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Interface for diagnostics controller
    /// </summary>
    public interface IDiagnosticsController
    {
        /// <summary>
        ///     Get diff between generated, verbatim and processed vocabularies.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetVocabulariesDiffAsZipFile();
    }
}