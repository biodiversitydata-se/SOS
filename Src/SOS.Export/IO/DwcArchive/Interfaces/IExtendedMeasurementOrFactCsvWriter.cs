using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using NReco.Csv;
using SOS.Export.Models;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Export.IO.DwcArchive.Interfaces
{
    public interface IExtendedMeasurementOrFactCsvWriter
    {
        /// <summary>
        /// Create a Emof CSV file.
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
        /// Create a headerless Emof CSV file.
        /// </summary>
        /// <param name="emofRows"></param>
        /// <param name="streamWriter"></param>
        /// <returns></returns>
        Task WriteHeaderlessEmofCsvFileAsync(
            IEnumerable<ExtendedMeasurementOrFactRow> emofRows,
            StreamWriter streamWriter);

        /// <summary>
        /// Write Emof header row.
        /// </summary>
        /// <param name="csvWriter"></param>
        void WriteHeaderRow(CsvWriter csvWriter);
    }
}