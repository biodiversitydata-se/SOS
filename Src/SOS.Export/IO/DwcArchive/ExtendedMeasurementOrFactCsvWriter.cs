using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Extensions;
using SOS.Export.IO.Csv.Converters;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Mappings;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.IO.DwcArchive
{
    public class ExtendedMeasurementOrFactCsvWriter : IExtendedMeasurementOrFactCsvWriter
    {
        private readonly ILogger<ExtendedMeasurementOrFactCsvWriter> _logger;

        public ExtendedMeasurementOrFactCsvWriter(ILogger<ExtendedMeasurementOrFactCsvWriter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateCsvFileAsync(
            FilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationRepository processedObservationRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var scrollResult = await processedObservationRepository.TypedScrollProjectParametersAsync(filter, null);
                await using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
                var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");

                // Write header row
                WriteHeaderRow(csvWriter);

                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var processedProjects = scrollResult.Records.ToArray();

                    // Convert observations to DwC format.
                    var emofRows = processedProjects.ToExtendedMeasurementOrFactRows(null);
                    //var dwcObservations = processedObservations.ToDarwinCore().ToArray();

                    // Write occurrence rows to CSV file.
                    foreach (var emofRow in emofRows)
                    {
                        WriteEmofRow(csvWriter, emofRow);
                    }
                    await streamWriter.FlushAsync();

                    // Get next batch of observations.
                    scrollResult = await processedObservationRepository.TypedScrollProjectParametersAsync(filter, scrollResult.ScrollId);
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
            csvWriter.WriteField(emofRow.OccurrenceID);
            csvWriter.WriteField(emofRow.MeasurementID);
            csvWriter.WriteField(DwcFormatter.RemoveNewLineTabs(emofRow.MeasurementType));
            csvWriter.WriteField(emofRow.MeasurementTypeID);
            csvWriter.WriteField(DwcFormatter.RemoveNewLineTabs(emofRow.MeasurementValue));
            csvWriter.WriteField(emofRow.MeasurementValueID);
            csvWriter.WriteField(DwcFormatter.RemoveNewLineTabs(emofRow.MeasurementAccuracy));
            csvWriter.WriteField(DwcFormatter.RemoveNewLineTabs(emofRow.MeasurementUnit));
            csvWriter.WriteField(emofRow.MeasurementUnitID);
            csvWriter.WriteField(emofRow.MeasurementDeterminedDate);
            csvWriter.WriteField(DwcFormatter.RemoveNewLineTabs(emofRow.MeasurementDeterminedBy));
            csvWriter.WriteField(DwcFormatter.RemoveNewLineTabs(emofRow.MeasurementRemarks));
            csvWriter.WriteField(DwcFormatter.RemoveNewLineTabs(emofRow.MeasurementMethod));

            csvWriter.NextRecord();
        }
    }
}