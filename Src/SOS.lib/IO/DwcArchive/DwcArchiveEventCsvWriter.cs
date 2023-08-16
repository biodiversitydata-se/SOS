using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.IO.DwcArchive
{
    public class DwcArchiveEventCsvWriter : IDwcArchiveEventCsvWriter
    {
        private readonly ILogger<DwcArchiveEventCsvWriter> _logger;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;

        public DwcArchiveEventCsvWriter(
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<DwcArchiveEventCsvWriter> logger)
        {
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateEventCsvFileAsync(
            SearchFilter filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationCoreRepository processedObservationRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var elasticRetrievalStopwatch = new Stopwatch();
                var csvWritingStopwatch = new Stopwatch();
                bool[] fieldsToWriteArray = FieldDescriptionHelper.CreateWriteFieldsArray(fieldDescriptions);
                elasticRetrievalStopwatch.Start();
                processedObservationRepository.LiveMode = true;
                var searchResult = await processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
                elasticRetrievalStopwatch.Stop();
                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(stream, "\t");

                var eventsWritten = new HashSet<string>();
                // Write header row
                WriteHeaderRow(csvFileHelper, fieldDescriptions);

                while (searchResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var searchResultTask = processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter);

                    // Fetch observations from ElasticSearch.
                    elasticRetrievalStopwatch.Start();
                    var processedObservations = searchResult.Records.ToArray();
                    elasticRetrievalStopwatch.Stop();

                    // Convert observations to DwC format.
                    LocalDateTimeConverterHelper.ConvertToLocalTime(processedObservations);
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);
                    var dwcObservations = processedObservations.ToDarwinCore().ToArray();

                    // Write occurrence rows to CSV file.
                    csvWritingStopwatch.Start();
                    foreach (var dwcObservation in dwcObservations)
                    {
                        if (!eventsWritten.TryGetValue(dwcObservation.Event.EventID, out var eventId))
                        {
                            WriteEventRow(csvFileHelper, dwcObservation, fieldsToWriteArray);
                            eventsWritten.Add(dwcObservation.Event.EventID);
                        }
                    }
                    await csvFileHelper.FlushAsync();
                    csvWritingStopwatch.Stop();

                    // Get next batch of observations.
                    elasticRetrievalStopwatch.Start();
                    searchResult = await searchResultTask;
                    elasticRetrievalStopwatch.Stop();
                }
                csvFileHelper.FinishWrite();

                stopwatch.Stop();
                _logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch.Elapsed.Duration()}. Elapsed time for CSV writing: {csvWritingStopwatch.Elapsed.Duration()}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch.Elapsed.Duration()}");
                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(CreateEventCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                throw;
            }
        }

        private static void WriteEventRow(
            CsvFileHelper csvFileHelper,
                    DarwinCore dwcObservation,
                    bool[] writeField)
        {
            if (writeField[(int)FieldDescriptionId.EventID]) csvFileHelper.WriteField(dwcObservation.Event.EventID);
            if (writeField[(int)FieldDescriptionId.ParentEventID]) csvFileHelper.WriteField(dwcObservation.Event.ParentEventID);
            if (writeField[(int)FieldDescriptionId.EventDate]) csvFileHelper.WriteField(dwcObservation.Event.EventDate);
            if (writeField[(int)FieldDescriptionId.VerbatimEventDate]) csvFileHelper.WriteField(dwcObservation.Event.VerbatimEventDate);
            if (writeField[(int)FieldDescriptionId.EventTime]) csvFileHelper.WriteField(dwcObservation.Event.EventTime);
            if (writeField[(int)FieldDescriptionId.EventRemarks]) csvFileHelper.WriteField(dwcObservation.Event.EventRemarks); 
            if (writeField[(int)FieldDescriptionId.FieldNotes]) csvFileHelper.WriteField(dwcObservation.Event.FieldNotes); 
            if (writeField[(int)FieldDescriptionId.FieldNumber]) csvFileHelper.WriteField(dwcObservation.Event.FieldNumber);
            if (writeField[(int)FieldDescriptionId.Habitat]) csvFileHelper.WriteField(dwcObservation.Event.Habitat);
            if (writeField[(int)FieldDescriptionId.SampleSizeValue]) csvFileHelper.WriteField(dwcObservation.Event.SampleSizeValue);
            if (writeField[(int)FieldDescriptionId.SampleSizeUnit]) csvFileHelper.WriteField(dwcObservation.Event.SampleSizeUnit);
            if (writeField[(int)FieldDescriptionId.SamplingEffort]) csvFileHelper.WriteField(dwcObservation.Event.SamplingEffort);
            if (writeField[(int)FieldDescriptionId.SamplingProtocol]) csvFileHelper.WriteField(dwcObservation.Event.SamplingProtocol);
            if (writeField[(int)FieldDescriptionId.Day]) csvFileHelper.WriteField(dwcObservation.Event.Day.HasValue ? dwcObservation.Event.Day.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Month]) csvFileHelper.WriteField(dwcObservation.Event.Month.HasValue ? dwcObservation.Event.Month.ToString() : null);
            if (writeField[(int)FieldDescriptionId.Year]) csvFileHelper.WriteField(dwcObservation.Event.Year.HasValue ? dwcObservation.Event.Year.ToString() : null);
            if (writeField[(int)FieldDescriptionId.EndDayOfYear]) csvFileHelper.WriteField(dwcObservation.Event.EndDayOfYear.HasValue ? dwcObservation.Event.EndDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.StartDayOfYear]) csvFileHelper.WriteField(dwcObservation.Event.StartDayOfYear.HasValue ? dwcObservation.Event.StartDayOfYear.ToString() : null);

            csvFileHelper.NextRecord();
        }

        private static string GetCoordinateUncertaintyInMetersValue(int? coordinateUncertaintyInMeters)
        {
            if (!coordinateUncertaintyInMeters.HasValue) return null;
            if (coordinateUncertaintyInMeters < 0)
            {
                return null;
            }
            if (coordinateUncertaintyInMeters.Value == 0)
            {
                return 1.ToString();
            }

            return coordinateUncertaintyInMeters.Value.ToString();
        }

        private void WriteHeaderRow(CsvFileHelper csvFileHelper, IEnumerable<FieldDescription> fieldDescriptions)
        {
            foreach (var fieldDescription in fieldDescriptions)
            {
                csvFileHelper.WriteField(fieldDescription.Name);
            }

            csvFileHelper.NextRecord();
        }

        public async Task WriteHeaderlessEventCsvFileAsync(
            IEnumerable<DarwinCore> dwcObservations,
            StreamWriter streamWriter,
            IEnumerable<FieldDescription> fieldDescriptions)
        {
            try
            {
                await Task.Run(() => {
                    bool[] fieldsToWriteArray = FieldDescriptionHelper.CreateWriteFieldsArray(fieldDescriptions);
                    using var csvFileHelper = new CsvFileHelper();
                    csvFileHelper.InitializeWrite(streamWriter, "\t");

                    // Write occurrence rows to CSV file.
                    foreach (var dwcObservation in dwcObservations)
                    {
                        WriteEventRow(csvFileHelper, dwcObservation, fieldsToWriteArray);
                    }
                    csvFileHelper.FinishWrite();
                });
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                throw;
            }
        }
    }
}