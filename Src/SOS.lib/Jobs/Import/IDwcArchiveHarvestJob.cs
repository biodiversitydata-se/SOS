using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IDwcArchiveHarvestJob
    {
        /// <summary>
        /// Run DwC-A harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync(string archivePath, int dataProviderId, IJobCancellationToken cancellationToken);
    }
}