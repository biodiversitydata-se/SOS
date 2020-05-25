using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="dataProviderIdOrIdentifier"></param>
        /// <returns></returns>
        Task<IActionResult> RunCopyDataProviderData(string dataProviderIdOrIdentifier);

        /// <summary>
        /// Activate instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        IActionResult RunSetActivateInstanceJob(byte instance);
    }
}
