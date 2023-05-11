using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IArtportalenDatasetMetadataHarvestJob
    {
        /// <summary>
        ///     Run harvest job.
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("Harvest Artportalen dataset metadata")]
        [Queue("high")]
        Task<bool> RunAsync();
    }
}