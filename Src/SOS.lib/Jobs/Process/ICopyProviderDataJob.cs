using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Process
{
    public interface ICopyProviderDataJob
    {
        /// <summary>
        ///     Copy data from active to inactive instance.
        /// </summary>
        /// <param name="dataProviderId"></param>
        /// <returns></returns>
        [Queue("high")]
        Task<bool> RunAsync(int dataProviderId);
    }
}