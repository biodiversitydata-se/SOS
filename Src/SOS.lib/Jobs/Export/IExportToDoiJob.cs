using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Export
{
    /// <summary>
    ///     Interface for DOI export job
    /// </summary>
    public interface IExportToDoiJob
    {
        /// <summary>
        /// Make a Doi of an an export file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Queue("medium")]
        Task<bool> RunAsync(string fileName, IJobCancellationToken cancellationToken);
    }
}