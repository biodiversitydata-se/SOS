using System.ComponentModel;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IApiUsageStatisticsHarvestJob
    {
        /// <summary>
        ///     Run harvest API usage statistics.
        /// </summary>
        /// <returns></returns>
        [DisplayName("Harvest API usage statistics")]
        Task<bool> RunAsync();
    }
}