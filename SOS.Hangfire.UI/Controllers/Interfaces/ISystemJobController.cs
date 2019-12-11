using Microsoft.AspNetCore.Mvc;

namespace SOS.Hangfire.UI.Controllers.Interfaces
{
    /// <summary>
    /// System job controller
    /// </summary>
    public interface ISystemJobController
    {
        /// <summary>
        /// Activate instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        IActionResult RunSetActivateInstanceJob(byte instance);
    }
}
