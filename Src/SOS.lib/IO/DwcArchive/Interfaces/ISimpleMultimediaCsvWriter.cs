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
    /// <summary>
    /// Simple multimedia extension CSV writer interface.
    /// </summary>
    public interface ISimpleMultimediaCsvWriter
    {
        /// <summary>
        /// Create a Simple Multimedia extensions CSV file.
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
        ///  Create a headerless Simple Multimedia extension CSV file.
        /// </summary>
        /// <param name="multimediaRows"></param>
        /// <param name="streamWriter"></param>
        /// <param name="eventBased"></param>
        void WriteHeaderlessCsvFile(
            IEnumerable<SimpleMultimediaRow> multimediaRows,
            StreamWriter streamWriter,
            bool eventBased = false);

        /// <summary>
        /// Write Simple multimedia extension header row.
        /// </summary>
        /// <param name="csvFileHelper"></param>
        /// <param name="eventBased"></param>
        void WriteHeaderRow(CsvFileHelper csvFileHelper, bool eventBased);
    }
}