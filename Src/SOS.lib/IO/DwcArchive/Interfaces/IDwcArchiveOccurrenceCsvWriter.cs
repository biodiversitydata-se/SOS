using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveOccurrenceCsvWriter
    {
        /// <summary>
        /// Creates a DwC occurrence CSV file.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="stream"></param>
        /// <param name="fieldDescriptions"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="leaveStreamOpen"></param>
        /// <param name="isEventCore"></param>
        /// <returns></returns>
        Task<int> CreateOccurrenceCsvFileAsync(
            SearchFilter filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationCoreRepository processedObservationRepository,
            IJobCancellationToken cancellationToken,
            bool leaveStreamOpen = false,
            bool isEventCore = false);

        /// <summary>
        /// Write Occurrence CSV file without headers using the stream writer.
        /// </summary>
        /// <param name="dwcObservations"></param>
        /// <param name="streamWriter"></param>
        /// <param name="fieldDescriptions"></param>
        /// <param name="isEventCore"></param>
        /// <returns></returns>
        Task WriteHeaderlessOccurrenceCsvFileAsync(
            IEnumerable<DarwinCore> dwcObservations,
            StreamWriter streamWriter,
            IEnumerable<FieldDescription> fieldDescriptions,
            bool isEventCore = false);

        /// <summary>
        /// Write Occurrence CSV header row.
        /// </summary>
        /// <param name="csvWriter"></param>
        /// <param name="fieldDescriptions"></param>
        void WriteHeaderRow(
            CsvFileHelper csvFileHelper, 
            IEnumerable<FieldDescription> fieldDescriptions);
    }
}