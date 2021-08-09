using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using NReco.Csv;
using SOS.Export.Models;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search;
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
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CreateCsvFileAsync(FilterBase filter, Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Create a headerless Simple Multimedia extension CSV file.
        /// </summary>
        /// <param name="multimediaRows"></param>
        /// <param name="streamWriter"></param>
        /// <returns></returns>
        Task WriteHeaderlessCsvFileAsync(
            IEnumerable<SimpleMultimediaRow> multimediaRows,
            StreamWriter streamWriter);

        /// <summary>
        /// Write Simple multimedia extension header row.
        /// </summary>
        /// <param name="csvWriter"></param>
        void WriteHeaderRow(CsvWriter csvWriter);
    }
}