using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    /// System controller interface
    /// </summary>
    public interface ISystemController
    {
        /// <summary>
        /// Get information about observation processing
        /// </summary>
        /// <param name="active">True: get information about last processing, false get information about previous processing</param>
        /// <returns>Meta data about processing. E.g, Start time, end time, number of observations processed...</returns>
        Task<IActionResult> GetProcessInfoAsync(bool active);
    }
}
