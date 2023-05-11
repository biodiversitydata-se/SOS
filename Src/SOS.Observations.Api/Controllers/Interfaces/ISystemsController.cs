using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     System controller interface
    /// </summary>
    public interface ISystemsController
    {
        /// <summary>
        /// Get build info
        /// </summary>
        /// <returns></returns>
      //  Task<IActionResult> GetBuildInfoAsync();

        /// <summary>
        /// Get copyright including system build time
        /// </summary>
        /// <returns></returns>
        IActionResult Copyright();

        /// <summary>
        ///     Get information about observation processing
        /// </summary>
        /// <param name="active">True: get information about last processing, false get information about previous processing</param>
        /// <returns>Meta data about processing. E.g, Start time, end time, number of observations processed...</returns>
        Task<IActionResult> GetProcessInfo(bool active);
    }
}