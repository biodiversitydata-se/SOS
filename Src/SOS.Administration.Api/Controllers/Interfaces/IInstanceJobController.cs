using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    /// System job controller
    /// </summary>
    public interface IInstanceJobController
    {
        /// <summary>
        /// Copy data from active to inactive instance
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        IActionResult RunCopyProviderData(DataProvider provider);

        /// <summary>
        /// Activate instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        IActionResult RunSetActivateInstanceJob(byte instance);
    }
}
