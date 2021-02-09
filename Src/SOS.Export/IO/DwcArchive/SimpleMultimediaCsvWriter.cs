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
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Export.IO.DwcArchive
{
    public class SimpleMultimediaCsvWriter : ISimpleMultimediaCsvWriter
    {
        private readonly ILogger<SimpleMultimediaCsvWriter> _logger;

        public SimpleMultimediaCsvWriter(ILogger<SimpleMultimediaCsvWriter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateCsvFileAsync(
            FilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var scrollResult = await processedPublicObservationRepository.ScrollMultimediaAsync(filter, null);
                var hasRecords = scrollResult?.Records?.Any() ?? false;
                if (!hasRecords) return false;

                await using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
                var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");

                // Write header row
                WriteHeaderRow(csvWriter);

                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var multimediaRows = scrollResult.Records.ToArray();

                    // Write occurrence rows to CSV file.
                    foreach (var row in multimediaRows)
                    {
                        WriteSimpleMultimediaRow(csvWriter, row);
                    }
                    await streamWriter.FlushAsync();

                    // Get next batch of observations.
                    scrollResult = await processedPublicObservationRepository.ScrollMultimediaAsync(filter, scrollResult.ScrollId);
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
                _logger.LogError(e, "Failed to create Simple Multimedia CSV file.");
                throw;
            }
        }

        public async Task WriteHeaderlessCsvFileAsync(
            IEnumerable<SimpleMultimediaRow> multimediaRows, 
            StreamWriter streamWriter)
        {
            try
            {
                var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");

                // Write simple multimedia rows to CSV file.
                foreach (var multimediaRow in multimediaRows)
                {
                    WriteSimpleMultimediaRow(csvWriter, multimediaRow);
                }

                await streamWriter.FlushAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Simple Multimedia CSV file.");
                throw;
            }
        }

        public void WriteHeaderRow(NReco.Csv.CsvWriter csvWriter)
        {
            var multimediaExtensionMetadata = ExtensionMetadata.SimpleMultimediaFactory.Create();
            foreach (var multimediaField in multimediaExtensionMetadata.Fields.OrderBy(field => field.Index))
            {
                csvWriter.WriteField(multimediaField.CSVColumnName);
            }

            csvWriter.NextRecord();
        }

        /// <summary>
        /// Write multimedia record to CSV file.
        /// </summary>
        /// <param name="csvWriter"></param>
        /// <param name="multimediaRow"></param>
        /// <remarks>The fields must be written in correct order. FieldDescriptionId sorted ascending.</remarks>
        private static void WriteSimpleMultimediaRow(
            NReco.Csv.CsvWriter csvWriter,
            SimpleMultimediaRow multimediaRow)
        {
            csvWriter.WriteField(multimediaRow.GbifID);
            csvWriter.WriteField(multimediaRow.Type);
            csvWriter.WriteField(multimediaRow.Format);
            csvWriter.WriteField(multimediaRow.Identifier);
            csvWriter.WriteField(multimediaRow.References);
            csvWriter.WriteField(multimediaRow.Title);
            csvWriter.WriteField(multimediaRow.Description);
            csvWriter.WriteField(multimediaRow.Source);
            csvWriter.WriteField(multimediaRow.Audience);
            csvWriter.WriteField(multimediaRow.Created);
            csvWriter.WriteField(multimediaRow.Creator);
            csvWriter.WriteField(multimediaRow.Contributor);
            csvWriter.WriteField(multimediaRow.Publisher);
            csvWriter.WriteField(multimediaRow.License);
            csvWriter.WriteField(multimediaRow.RightsHolder);

            csvWriter.NextRecord();
        }
    }
}