using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     System job controller
    /// </summary>
    public interface IInstanceJobController
    {        
        /// <summary>
        ///     Activate instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        IActionResult RunSetActivateInstanceJob(byte instance);
    }
}