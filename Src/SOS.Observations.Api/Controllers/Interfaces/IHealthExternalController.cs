using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Health check controller interface
    /// </summary>
    public interface IHealthExternalController
    {
        /// <summary>
        /// Get system health status
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> HealthCheck();
    }
}