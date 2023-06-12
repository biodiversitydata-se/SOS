using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveEventCsvWriter
    {
        /// <summary>
        ///     Creates a DwC occurrence CSV file.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="stream"></param>
        /// <param name="fieldDescriptions"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CreateEventCsvFileAsync(
            SearchFilter filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationCoreRepository processedObservationRepository,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Write Occurrence CSV file without headers using the stream writer.
        /// </summary>
        /// <param name="dwcObservations"></param>
        /// <param name="streamWriter"></param>
        /// <param name="fieldDescriptions"></param>
        /// <returns></returns>
        Task WriteHeaderlessEventCsvFileAsync(
            IEnumerable<DarwinCore> dwcObservations,
            StreamWriter streamWriter,
            IEnumerable<FieldDescription> fieldDescriptions);

    }
}