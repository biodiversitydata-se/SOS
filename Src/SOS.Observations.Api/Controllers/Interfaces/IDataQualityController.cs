using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Area controller interface
    /// </summary>
    public interface IDataQualityController
    {
        /// <summary>
        ///     Data quality report.
        /// </summary>
        /// <returns>List of observations that can be duplicates</returns>
        Task<IActionResult> GetReport();
    }
}