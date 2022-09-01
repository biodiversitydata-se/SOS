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
        /// <param name="fieldDescriptions"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CreateCsvFileAsync(SearchFilterBase filter, Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationCoreRepository processedObservationRepository,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Create a headerless Simple Multimedia extension CSV file.
        /// </summary>
        /// <param name="multimediaRows"></param>
        /// <param name="streamWriter"></param>
        /// <returns></returns>
        void WriteHeaderlessCsvFile(
            IEnumerable<SimpleMultimediaRow> multimediaRows,
            StreamWriter streamWriter);

        /// <summary>
        /// Write Simple multimedia extension header row.
        /// </summary>
        /// <param name="csvFileHelper"></param>
        void WriteHeaderRow(CsvFileHelper csvFileHelper);
    }
}