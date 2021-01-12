using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Processors.Interfaces
{
    /// <summary>
    ///     Process base factory
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Process observations
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="protectedObservations"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            bool protectedObservations,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);
    }
}