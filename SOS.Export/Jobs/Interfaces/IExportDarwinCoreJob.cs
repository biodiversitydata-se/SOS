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
        /// <returns></returns>
        Task<bool> Run(IJobCancellationToken cancellationToken);
    }
}
