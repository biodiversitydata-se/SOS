using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed;

namespace SOS.Process.Processors.Interfaces
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
        Task<ProcessingStatus> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken);

    }
}
