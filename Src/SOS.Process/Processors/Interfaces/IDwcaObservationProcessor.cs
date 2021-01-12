using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Processors.Interfaces
{
    /// <summary>
    ///     DwC-A observation processor
    /// </summary>
    public interface IDwcaObservationProcessor : IProcessor
    {
        Task<bool> DoesVerbatimDataExist();

        /// <inheritdoc />
        Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            bool protectedObservations,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);
    }
}