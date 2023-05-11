using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;

namespace SOS.Lib.Jobs.Import
{
    public interface IDwcArchiveHarvestJob : IHarvestJob
    {
        /// <summary>
        ///     Run DwC-A harvest
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("Harvest observations from a DwC-A file")]
        [Queue("high")]
        Task<bool> RunAsync(
            int dataProviderId,
            string archivePath,
            DwcaTarget target,
            IJobCancellationToken cancellationToken);
    }
}