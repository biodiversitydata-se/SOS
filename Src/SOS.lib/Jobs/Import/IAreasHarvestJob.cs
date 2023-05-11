using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IAreasHarvestJob
    {
        /// <summary>
        ///     Run geo harvest
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("Harvest areas")]
        [Queue("high")]
        Task<bool> RunAsync();
    }
}