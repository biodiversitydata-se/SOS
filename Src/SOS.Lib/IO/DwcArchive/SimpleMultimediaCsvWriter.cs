﻿using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.IO.DwcArchive
{
    public class SimpleMultimediaCsvWriter : ISimpleMultimediaCsvWriter
    {
        private readonly ILogger<SimpleMultimediaCsvWriter> _logger;

        public SimpleMultimediaCsvWriter(ILogger<SimpleMultimediaCsvWriter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateCsvFileAsync(
            SearchFilterBase filter,
            Stream stream,
            IProcessedObservationCoreRepository processedObservationRepository,
            IJobCancellationToken cancellationToken,
            bool isEventCore = false)
        {
            try
            {
                var searchResult = await processedObservationRepository.GetMultimediaBySearchAfterAsync(filter);
                var hasRecords = searchResult?.Records?.Any() ?? false;
                if (!hasRecords) return false;

                var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(stream, "\t");
                // Write header row
                WriteHeaderRow(csvFileHelper, isEventCore);

                while (searchResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var searchResultTask = processedObservationRepository.GetMultimediaBySearchAfterAsync(filter, searchResult.PointInTimeId, searchResult.SearchAfter);

                    // Fetch observations from ElasticSearch.
                    var multimediaRows = searchResult.Records.ToArray();

                    // Write occurrence rows to CSV file.
                    foreach (var row in multimediaRows)
                    {
                        WriteSimpleMultimediaRow(csvFileHelper, row, isEventCore);
                    }
                    await csvFileHelper.FlushAsync();

                    // Get next batch of observations.
                    searchResult = await searchResultTask;
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
                _logger.LogError(e, "Failed to create Simple Multimedia CSV file.");
                throw;
            }
        }

        public void WriteHeaderlessCsvFile(
            IEnumerable<SimpleMultimediaRow> multimediaRows,
            StreamWriter streamWriter,
            bool eventBased = false)
        {
            try
            {
                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(streamWriter, "\t");

                // Write simple multimedia rows to CSV file.
                foreach (var multimediaRow in multimediaRows)
                {
                    WriteSimpleMultimediaRow(csvFileHelper, multimediaRow, eventBased);
                }

                csvFileHelper.FinishWrite();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Simple Multimedia CSV file.");
                throw;
            }
        }

        public void WriteHeaderRow(CsvFileHelper csvFileHelper, bool eventBased = false)
        {
            var multimediaExtensionMetadata = ExtensionMetadata.SimpleMultimediaFactory.Create(eventBased);
            foreach (var multimediaField in multimediaExtensionMetadata.Fields.OrderBy(field => field.Index))
            {
                csvFileHelper.WriteField(multimediaField.CSVColumnName);
            }

            csvFileHelper.NextRecord();
        }

        /// <summary>
        /// Write multimedia record to CSV file.
        /// </summary>
        /// <param name="csvFileHelper"></param>
        /// <param name="multimediaRow"></param>
        /// <param name="eventBased"></param>
        /// <remarks>The fields must be written in correct order. FieldDescriptionId sorted ascending.</remarks>
        private static void WriteSimpleMultimediaRow(
            CsvFileHelper csvFileHelper,
            SimpleMultimediaRow multimediaRow,
            bool eventBased = false)
        {
            if (eventBased)
            {
                csvFileHelper.WriteField(multimediaRow.EventId);
            }

            csvFileHelper.WriteField(multimediaRow.OccurrenceId);
            csvFileHelper.WriteField(multimediaRow.Type);
            csvFileHelper.WriteField(multimediaRow.Format);
            csvFileHelper.WriteField(multimediaRow.Identifier);
            csvFileHelper.WriteField(multimediaRow.References);
            csvFileHelper.WriteField(multimediaRow.Title);
            csvFileHelper.WriteField(multimediaRow.Description);
            csvFileHelper.WriteField(multimediaRow.Source);
            csvFileHelper.WriteField(multimediaRow.Audience);
            csvFileHelper.WriteField(multimediaRow.Created);
            csvFileHelper.WriteField(multimediaRow.Creator);
            csvFileHelper.WriteField(multimediaRow.Contributor);
            csvFileHelper.WriteField(multimediaRow.Publisher);
            csvFileHelper.WriteField(multimediaRow.License);
            csvFileHelper.WriteField(multimediaRow.RightsHolder);

            csvFileHelper.NextRecord();
        }
    }
}