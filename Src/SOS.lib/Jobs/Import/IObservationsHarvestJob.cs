using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IObservationsHarvestJob
    {
        /// <summary>
        /// Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        /// Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="harvestDataProviderIdOrIdentifiers"></param>
        /// <param name="processDataProviderIdOrIdentifiers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(
            List<string> harvestDataProviderIdOrIdentifiers, 
            List<string> processDataProviderIdOrIdentifiers, 
            IJobCancellationToken cancellationToken);
    }
}
