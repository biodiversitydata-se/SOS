using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Search.Service.Controllers.Interfaces
{
    /// <summary>
    /// Export job controller
    /// </summary>
    public interface IDOIController
    {
        /// <summary>
        /// Run export DOI job
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IActionResult RunDOIExportJob(AdvancedFilter filter);
    }
}
