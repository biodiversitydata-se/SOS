using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Search.Filters;

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
        /// <param name="processedObservationRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if any records is written to the file; otherwise false.</returns>
        public async Task<bool> CreateCsvFileAsync(
            SearchFilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationRepository processedObservationRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var scrollResult = await processedObservationRepository.ScrollMeasurementOrFactsAsync(filter, null);
                if (!scrollResult?.Records?.Any() ?? true) return false;

                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(stream, "\t");

                // Write header row
                WriteHeaderRow(csvFileHelper);

                while (scrollResult.Records.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var emofRows = scrollResult.Records.ToArray();

                    // Write occurrence rows to CSV file.
                    foreach (var emofRow in emofRows)
                    {
                        WriteEmofRow(csvFileHelper, emofRow);
                    }
                    await csvFileHelper.FlushAsync();

                    // Get next batch of observations.
                    scrollResult = await processedObservationRepository.ScrollMeasurementOrFactsAsync(filter, scrollResult.ScrollId);
                }

                csvFileHelper.FinishWrite();
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
            StreamWriter streamWriter,
            bool writeEventId = false)
        {
            try
            {
                if (!emofRows?.Any() ?? true)
                {
                    return;
                }

                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(streamWriter, "\t");

                // Write Emof rows to CSV file.
                foreach (var emofRow in emofRows)
                {
                    WriteEmofRow(csvFileHelper, emofRow, writeEventId);
                }

                csvFileHelper.FinishWrite();
                //_logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch.Elapsed.Duration()}. Elapsed time for CSV writing: {csvWritingStopwatch.Elapsed.Duration()}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch.Elapsed.Duration()}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Emof txt file.");
                throw;
            }
        }

        public void WriteHeaderRow(CsvFileHelper csvFileHelper, bool isEventCore = false)
        {
            var emofExtensionMetadata = ExtensionMetadata.EmofFactory.Create(isEventCore);
            foreach (var emofField in emofExtensionMetadata.Fields.OrderBy(field => field.Index))
            {
                csvFileHelper.WriteField(emofField.CSVColumnName);
            }

            csvFileHelper.NextRecord();
        }

        /// <summary>
        /// Write Emof record to CSV file.
        /// </summary>
        /// <param name="csvFileHelper"></param>
        /// <param name="emofRow"></param>
        /// <param name="writeEventId"></param>
        /// <remarks>The fields must be written in correct order. FieldDescriptionId sorted ascending.</remarks>
        private static void WriteEmofRow(
            CsvFileHelper csvFileHelper,
            ExtendedMeasurementOrFactRow emofRow,
            bool writeEventId = false)
        {
            if (emofRow == null)
            {
                return;
            }

            if (writeEventId) csvFileHelper.WriteField(emofRow.EventId);
            csvFileHelper.WriteField(emofRow.OccurrenceID);
            csvFileHelper.WriteField(emofRow.MeasurementID);
            csvFileHelper.WriteField(emofRow.MeasurementType);
            csvFileHelper.WriteField(emofRow.MeasurementTypeID);
            csvFileHelper.WriteField(emofRow.MeasurementValue);
            csvFileHelper.WriteField(emofRow.MeasurementValueID);
            csvFileHelper.WriteField(emofRow.MeasurementAccuracy);
            csvFileHelper.WriteField(emofRow.MeasurementUnit);
            csvFileHelper.WriteField(emofRow.MeasurementUnitID);
            csvFileHelper.WriteField(emofRow.MeasurementDeterminedDate);
            csvFileHelper.WriteField(emofRow.MeasurementDeterminedBy);
            csvFileHelper.WriteField(emofRow.MeasurementRemarks);
            csvFileHelper.WriteField(emofRow.MeasurementMethod);

            csvFileHelper.NextRecord();
        }
    }
}