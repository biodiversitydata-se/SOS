using Hangfire;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Processors.Interfaces
{
    /// <summary>
    /// Event processor interface.
    /// </summary>
    public interface IEventProcessor
    {
        /// <summary>
        /// Process datasets
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);
    }
}