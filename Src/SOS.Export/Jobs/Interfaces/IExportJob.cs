using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Search;

namespace SOS.Export.Jobs.Interfaces
{
    /// <summary>
    /// Interface for DOI export job
    /// </summary>
    public interface IExportJob
    {
        /// <summary>
        /// Run DOI export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(ExportFilter filter, string email, IJobCancellationToken cancellationToken);
    }
}
