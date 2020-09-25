using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.DOI.Controllers.Interfaces
{
    public interface IDoiController
    {
        /// <summary>
        ///  Get DOI meta data
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        Task<IActionResult> GetMetadata([FromRoute] string prefix, [FromRoute] string suffix);

        /// <summary>
        /// Get DOI download URL
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        IActionResult GetDOIFileUrl(string suffix);

        /// <summary>
        /// Search for DOI's
        /// </summary>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        Task<IActionResult> SearchMetadata([FromQuery] string searchFor);
    }
}
