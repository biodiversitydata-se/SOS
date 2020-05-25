using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IDwcArchiveHarvestJob : IHarvestJob
    {
        /// <summary>
        /// Run DwC-A harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync(int dataProviderId, string archivePath, IJobCancellationToken cancellationToken);
    }
}