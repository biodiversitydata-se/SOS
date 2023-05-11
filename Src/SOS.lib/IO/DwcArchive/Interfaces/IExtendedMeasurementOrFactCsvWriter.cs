using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.IO.DwcArchive.Interfaces
{
    public interface IExtendedMeasurementOrFactCsvWriter
    {
        /// <summary>
        /// Create a Emof CSV file.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="stream"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="isEventCore"></param>
        /// <returns></returns>
        Task<bool> CreateCsvFileAsync(SearchFilterBase filter, Stream stream,
            IProcessedObservationCoreRepository processedObservationRepository,
            IJobCancellationToken cancellationToken,
            bool isEventCore = false);

        /// <summary>
        /// Create a headerless Emof CSV file.
        /// </summary>
        /// <param name="emofRows"></param>
        /// <param name="streamWriter"></param>
        /// <param name="writeEventId"></param>
        /// <returns></returns>
        Task WriteHeaderlessEmofCsvFileAsync(
            IEnumerable<ExtendedMeasurementOrFactRow> emofRows,
            StreamWriter streamWriter,
            bool writeEventId = false);

        /// <summary>
        /// Write Emof header row.
        /// </summary>
        /// <param name="csvFileHelper"></param>
        /// <param name="isEventCore"></param>
        void WriteHeaderRow(CsvFileHelper csvFileHelper, bool isEventCore = false);
    }
}