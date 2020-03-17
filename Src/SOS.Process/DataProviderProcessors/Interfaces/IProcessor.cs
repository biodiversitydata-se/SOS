using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Process.DataProviderProcessors.Interfaces
{
    /// <summary>
    /// Process base factory
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Process sightings
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<RunInfo> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken);

    }
}
