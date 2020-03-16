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
        ///  Get process information 
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        Task<IActionResult> GetProcessInfoAsync(bool active);
    }
}
