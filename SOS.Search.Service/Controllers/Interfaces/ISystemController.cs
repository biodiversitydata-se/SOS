using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Search.Service.Controllers.Interfaces
{
    /// <summary>
    /// System controller interface
    /// </summary>
    public interface ISystemController
    {
        /// <summary>
        /// Get process information of active instance
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetCurrentProcessInfoAsync();
    }
}
