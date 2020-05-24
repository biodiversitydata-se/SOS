using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Processors.Interfaces
{
    /// <summary>
    /// DwC-A observation processor
    /// </summary>
    public interface IDwcaObservationProcessor : IProcessor
    {
        Task<bool> DoesVerbatimDataExist();

        Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken);
    }
}