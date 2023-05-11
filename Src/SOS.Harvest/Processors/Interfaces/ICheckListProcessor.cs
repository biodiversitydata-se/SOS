using Hangfire;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Processors.Interfaces
{
    /// <summary>
    ///     Process base factory
    /// </summary>
    public interface IChecklistProcessor
    {
        /// <summary>
        /// Process observations
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);
    }
}