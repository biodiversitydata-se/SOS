using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Process job controller
    /// </summary>
    public interface IProcessJobController
    {
        /// <summary>
        ///     Run process job
        /// </summary>        
        /// <returns></returns>
        Task<IActionResult> RunProcessJob();

        /// <summary>
        ///     Run process taxa job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunProcessTaxaJob();
    }
}