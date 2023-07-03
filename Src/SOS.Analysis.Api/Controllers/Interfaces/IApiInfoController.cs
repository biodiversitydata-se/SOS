using Microsoft.AspNetCore.Mvc;

namespace SOS.Analysis.Api.Controllers.Interfaces
{
    public interface IApiInfoController
    {
        /// <summary>
        /// Get api information
        /// </summary>
        /// <returns></returns>
        IActionResult GetApiInfoAsync();
    }
}
