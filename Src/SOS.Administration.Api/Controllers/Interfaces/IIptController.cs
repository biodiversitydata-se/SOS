
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    public interface IIptController
    {
        /// <summary>
        /// Get all resources
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetResourcesAsync();
    }
}
