using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Export job controller
    /// </summary>
    public interface IJobsController
    {
        /// <summary>
        ///     Get status of job
        /// </summary>
        /// <param name="jobId">Job id returned when export was requested</param>
        /// <returns></returns>
        IActionResult GetStatus(string jobId);
    }
}