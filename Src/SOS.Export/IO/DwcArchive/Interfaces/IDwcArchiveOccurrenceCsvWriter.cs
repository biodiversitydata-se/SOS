using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search;

namespace SOS.Export.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveOccurrenceCsvWriter
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
        Task<bool> CreateOccurrenceCsvFileAsync(
            FilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationRepository processedObservationRepository,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Write Occurrence CSV file without headers using the stream writer.
        /// </summary>
        /// <param name="dwcObservations"></param>
        /// <param name="streamWriter"></param>
        /// <param name="fieldDescriptions"></param>
        /// <returns></returns>
        Task CreateOccurrenceCsvFileAsync(
            IEnumerable<DarwinCore> dwcObservations,
            StreamWriter streamWriter,
            IEnumerable<FieldDescription> fieldDescriptions);

        /// <summary>
        /// Write Occurrence CSV header row.
        /// </summary>
        /// <param name="csvWriter"></param>
        /// <param name="fieldDescriptions"></param>
        void WriteHeaderRow(
            NReco.Csv.CsvWriter csvWriter, 
            IEnumerable<FieldDescription> fieldDescriptions);
    }
}