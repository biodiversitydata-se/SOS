using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessJob
    {
        /// <summary>
        /// Process data
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="cleanStart"></param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(int sources, bool cleanStart, bool toggleInstanceOnSuccess, IJobCancellationToken cancellationToken);
    }
}
