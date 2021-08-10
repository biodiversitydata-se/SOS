using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.IO.DwcArchive
{
    public class ExtendedMeasurementOrFactCsvWriter : IExtendedMeasurementOrFactCsvWriter
    {
        private readonly ILogger<ExtendedMeasurementOrFactCsvWriter> _logger;

        public ExtendedMeasurementOrFactCsvWriter(ILogger<ExtendedMeasurementOrFactCsvWriter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Writes Emof rows to file.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="stream"></param>
        /// <param name="fieldDescriptions"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if any records is written to the file; otherwise false.</returns>
        public async Task<bool> CreateCsvFileAsync(
            FilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var scrollResult = await processedPublicObservationRepository.ScrollMeasurementOrFactsAsync(filter, null);
                if (!scrollResult?.Records?.Any() ?? true) return false;

                await using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
                var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");

                // Write header row
                WriteHeaderRow(csvWriter);

                while (scrollResult.Records.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var emofRows = scrollResult.Records.ToArray();

                    // Write occurrence rows to CSV file.
                    foreach (var emofRow in emofRows)
                    {
                        WriteEmofRow(csvWriter, emofRow);
                    }
                    await streamWriter.FlushAsync();

                    // Get next batch of observations.
                    scrollResult = await processedPublicObservationRepository.ScrollMeasurementOrFactsAsync(filter, scrollResult.ScrollId);
                }

                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(CreateCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create EMOF CSV file.");
                throw;
            }
        }

        public async Task WriteHeaderlessEmofCsvFileAsync(
            IEnumerable<ExtendedMeasurementOrFactRow> emofRows, 
            StreamWriter streamWriter)
        {
            try
            {
                if (!emofRows?.Any() ?? true)
                {
                    return;
                }

                var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");

                // Write Emof rows to CSV file.
                foreach (var emofRow in emofRows)
                {
                    WriteEmofRow(csvWriter, emofRow);
                }

                await streamWriter.FlushAsync();
                //_logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch.Elapsed.Duration()}. Elapsed time for CSV writing: {csvWritingStopwatch.Elapsed.Duration()}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch.Elapsed.Duration()}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Emof CSV file.");
                throw;
            }
        }

        public void WriteHeaderRow(NReco.Csv.CsvWriter csvWriter)
        {
            var emofExtensionMetadata = ExtensionMetadata.EmofFactory.Create();
            foreach (var emofField in emofExtensionMetadata.Fields.OrderBy(field => field.Index))
            {
                csvWriter.WriteField(emofField.CSVColumnName);
            }

            csvWriter.NextRecord();
        }

        /// <summary>
        /// Write Emof record to CSV file.
        /// </summary>
        /// <param name="csvWriter"></param>
        /// <param name="emofRow"></param>
        /// <remarks>The fields must be written in correct order. FieldDescriptionId sorted ascending.</remarks>
        private static void WriteEmofRow(
            NReco.Csv.CsvWriter csvWriter,
            ExtendedMeasurementOrFactRow emofRow)
        {
            if (emofRow == null)
            {
                return;
            }

            csvWriter.WriteField(emofRow.OccurrenceID);
            csvWriter.WriteField(emofRow.MeasurementID);
            csvWriter.WriteField(emofRow.MeasurementType.RemoveNewLineTabs());
            csvWriter.WriteField(emofRow.MeasurementTypeID);
            csvWriter.WriteField(emofRow.MeasurementValue.RemoveNewLineTabs());
            csvWriter.WriteField(emofRow.MeasurementValueID);
            csvWriter.WriteField(emofRow.MeasurementAccuracy.RemoveNewLineTabs());
            csvWriter.WriteField(emofRow.MeasurementUnit.RemoveNewLineTabs());
            csvWriter.WriteField(emofRow.MeasurementUnitID);
            csvWriter.WriteField(emofRow.MeasurementDeterminedDate);
            csvWriter.WriteField(emofRow.MeasurementDeterminedBy.RemoveNewLineTabs());
            csvWriter.WriteField(emofRow.MeasurementRemarks.RemoveNewLineTabs());
            csvWriter.WriteField(emofRow.MeasurementMethod.RemoveNewLineTabs());

            csvWriter.NextRecord();
        }
    }
}