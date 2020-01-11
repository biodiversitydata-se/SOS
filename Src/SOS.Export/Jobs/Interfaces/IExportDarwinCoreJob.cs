using System.Threading.Tasks;
using Hangfire;

namespace SOS.Export.Jobs.Interfaces
{
    /// <summary>
    /// Interface for export DC job
    /// </summary>
    public interface IExportDarwinCoreJob
    {
        /// <summary>
        /// Run export Darwin core job
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(string fileName, IJobCancellationToken cancellationToken);
    }
}
