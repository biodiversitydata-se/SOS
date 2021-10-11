using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.IO.DwcArchive
{
    public class DwcArchiveEventFileWriter : IDwcArchiveEventFileWriter
    {
        private readonly IDwcArchiveOccurrenceCsvWriter _dwcArchiveOccurrenceCsvWriter;
        private readonly IDwcArchiveEventCsvWriter _dwcArchiveEventCsvWriter;
        private readonly IExtendedMeasurementOrFactCsvWriter _extendedMeasurementOrFactCsvWriter;
        private readonly ISimpleMultimediaCsvWriter _simpleMultimediaCsvWriter;
        private readonly IFileService _fileService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly ILogger<DwcArchiveFileWriter> _logger;
        private readonly object _eventsLock = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dwcArchiveOccurrenceCsvWriter"></param>
        /// <param name="dwcArchiveEventCsvWriter"></param>
        /// <param name="extendedMeasurementOrFactCsvWriter"></param>
        /// <param name="simpleMultimediaCsvWriter"></param>
        /// <param name="fileService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="logger"></param>
        public DwcArchiveEventFileWriter(IDwcArchiveOccurrenceCsvWriter dwcArchiveOccurrenceCsvWriter,
            IDwcArchiveEventCsvWriter dwcArchiveEventCsvWriter,
            IExtendedMeasurementOrFactCsvWriter extendedMeasurementOrFactCsvWriter,
            ISimpleMultimediaCsvWriter simpleMultimediaCsvWriter,
            IFileService fileService,
            IDataProviderRepository dataProviderRepository,
            ILogger<DwcArchiveFileWriter> logger)
        {
            _dwcArchiveOccurrenceCsvWriter = dwcArchiveOccurrenceCsvWriter ??
                                             throw new ArgumentNullException(nameof(dwcArchiveOccurrenceCsvWriter));
            _dwcArchiveEventCsvWriter = dwcArchiveEventCsvWriter ??
                                        throw new ArgumentNullException(nameof(dwcArchiveEventCsvWriter));
            _extendedMeasurementOrFactCsvWriter = extendedMeasurementOrFactCsvWriter ??
                                                  throw new ArgumentNullException(
                                                      nameof(extendedMeasurementOrFactCsvWriter));
            _simpleMultimediaCsvWriter = simpleMultimediaCsvWriter ?? throw new ArgumentNullException(nameof(simpleMultimediaCsvWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _dataProviderRepository =
                dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private List<Observation> GetNonProcessedObservationEvents(ICollection<Observation> observations, HashSet<string> writtenEvents)
        {
            var nonProcessedObservationEvents = new List<Observation>();
            lock (_eventsLock)
            {
                foreach (var observation in observations)
                {
                    string eventId = observation?.Event?.EventId;
                    if (!writtenEvents.Contains(eventId))
                    {
                        writtenEvents.Add(eventId);
                        nonProcessedObservationEvents.Add(observation);
                    }
                }
            }

            return nonProcessedObservationEvents;
        }

        private List<ExtendedMeasurementOrFactRow> GetNonWrittenMeasurements(
            IEnumerable<ExtendedMeasurementOrFactRow> emofRows,
            HashSet<string> writtenEventsMeasurements)
        {
            var nonWrittenMeasurements = new List<ExtendedMeasurementOrFactRow>();
            lock (_eventsLock)
            {
                foreach (var row in emofRows)
                {
                    string key = $"{row.EventId}-{row.OccurrenceID}-{row.MeasurementType}";
                    if (!writtenEventsMeasurements.Contains(key))
                    {
                        writtenEventsMeasurements.Add(key);
                        nonWrittenMeasurements.Add(row);
                    }
                }
            }

            return nonWrittenMeasurements;
        }


        public async Task WriteHeaderlessEventDwcaFiles(
            ICollection<Observation> processedObservations,
            Dictionary<DwcaEventFilePart, string> eventFilePathByFilePart,
            WrittenEventSets writtenEventsData)
        {
            if (!processedObservations?.Any() ?? true)
            {
                return;
            }

            var nonProcessedObservationEvents = GetNonProcessedObservationEvents(processedObservations, writtenEventsData.WrittenEvents);
            var eventFieldDescriptions = FieldDescriptionHelper.GetAllDwcEventCoreFieldDescriptions();

            // Create Event CSV file
            string eventCsvFilePath = eventFilePathByFilePart[DwcaEventFilePart.Event];
            var dwcObservations = nonProcessedObservationEvents.ToDarwinCore();
            await using var eventFileStream = File.AppendText(eventCsvFilePath);
            await _dwcArchiveEventCsvWriter.WriteHeaderlessEventCsvFileAsync(
                dwcObservations,
                eventFileStream,
                eventFieldDescriptions);

            // Create EMOF Event CSV file
            string emofCsvFilePath = eventFilePathByFilePart[DwcaEventFilePart.Emof];
            var emofRows = processedObservations.ToExtendedMeasurementOrFactRows(true);
            emofRows = GetNonWrittenMeasurements(emofRows, writtenEventsData.WrittenMeasurements);
            if (emofRows != null && emofRows.Any())
            {
                await using StreamWriter emofFileStream = File.AppendText(emofCsvFilePath);
                await _extendedMeasurementOrFactCsvWriter.WriteHeaderlessEmofCsvFileAsync(
                    emofRows,
                    emofFileStream,
                    true);
            }

            // Create Occurrence event CSV file - exclude event data
            string occurrenceCsvFilePath = eventFilePathByFilePart[DwcaEventFilePart.Occurrence];
            var dwcEventObservations = processedObservations.ToDarwinCore();
            var occurrenceFieldDescriptions = FieldDescriptionHelper.GetAllDwcEventCoreOccurrenceFieldDescriptions();
            await using StreamWriter occurrenceFileStream = File.AppendText(occurrenceCsvFilePath);
            await _dwcArchiveOccurrenceCsvWriter.WriteHeaderlessOccurrenceCsvFileAsync(
                dwcEventObservations,
                occurrenceFileStream,
                occurrenceFieldDescriptions,
                true);

            //// Create Multimedia CSV file
            // todo
            //string multimediaCsvFilePath = filePathByFilePart[DwcaEventFilePart.Multimedia];
            //var multimediaRows = processedObservations.ToSimpleMultimediaRows();
            //if (multimediaRows != null && multimediaRows.Any())
            //{
            //    await using StreamWriter multimediaFileStream = File.AppendText(multimediaCsvFilePath);
            //    await _simpleMultimediaCsvWriter.WriteHeaderlessCsvFileAsync(
            //        multimediaRows,
            //        multimediaFileStream);
            //}
        }

        public async Task<string> CreateEventDwcArchiveFileAsync(
            DataProvider dataProvider,
            string exportFolderPath,
            DwcaFilePartsInfo dwcaFilePartsInfo)
        {
            string tempFilePath = null;
            try
            {
                tempFilePath = Path.Combine(exportFolderPath, $"Temp_{Path.GetRandomFileName()}.dwca.zip");
                var filePath = Path.Combine(exportFolderPath, $"{dwcaFilePartsInfo.DataProvider.Identifier}-event.dwca.zip");

                // Create the DwC-A file
                await CreateEventDwcArchiveFileAsync(dataProvider, new[] { dwcaFilePartsInfo }, tempFilePath);

                File.Move(tempFilePath, filePath, true);
                _logger.LogInformation($"A new .zip({filePath}) was created.");

                return filePath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Creating DwC-A .zip for {dwcaFilePartsInfo?.DataProvider} failed");
                throw;
            }
            finally
            {
                if (tempFilePath != null && File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        private async Task CreateEventDwcArchiveFileAsync(DataProvider dataProvider,
            IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos, string tempFilePath)
        {
            var fieldDescriptions = FieldDescriptionHelper.GetAllDwcEventCoreFieldDescriptions().ToList();
            var eventCoreOccurrenceFieldDescriptions =
                FieldDescriptionHelper.GetAllDwcEventCoreOccurrenceFieldDescriptions().ToList();
           
            using var archive = ZipFile.Open(tempFilePath, ZipArchiveMode.Create);
            var dwcExtensions = new List<DwcaEventFilePart>();

            // Create event.csv
            var eventFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-event*");
            await using var eventFileStream = archive.CreateEntry("event.csv", CompressionLevel.Fastest).Open();

            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(eventFileStream, "\t");

            await WriteEventHeaderRow(csvFileHelper);
            foreach (var filePath in eventFilePaths)
            {
                await using var readStream = File.OpenRead(filePath);
                await readStream.CopyToAsync(eventFileStream);
                readStream.Close();
            }
            eventFileStream.Close();

            // Create occurrence.csv
            var occurrenceFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-occurrence*");
            if (occurrenceFilePaths.Any())
            {
                dwcExtensions.Add(DwcaEventFilePart.Occurrence);
                await using var occurrenceFileStream = archive.CreateEntry("occurrence.csv", CompressionLevel.Fastest).Open();
                WriteEventOccurrenceHeaderRow(occurrenceFileStream);
                foreach (var filePath in occurrenceFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(occurrenceFileStream);
                    readStream.Close();
                }
                occurrenceFileStream.Close();
            }

            // Create emof.csv
            var emofFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-emof*");
            if (emofFilePaths.Any())
            {
                dwcExtensions.Add(DwcaEventFilePart.Emof);
                await using var extendedMeasurementOrFactFileStream = archive.CreateEntry("extendedMeasurementOrFact.csv", CompressionLevel.Fastest).Open();
                WriteEmofHeaderRow(extendedMeasurementOrFactFileStream, true);
                foreach (var filePath in emofFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(extendedMeasurementOrFactFileStream);
                    readStream.Close();
                }
                extendedMeasurementOrFactFileStream.Close();
            }

            // Create multimedia.csv
            var multimediaFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-multimedia*");
            if (multimediaFilePaths.Any())
            {
                dwcExtensions.Add(DwcaEventFilePart.Multimedia);
                await using var multimediaFileStream = archive.CreateEntry("multimedia.csv", CompressionLevel.Fastest).Open();
                await WriteMultimediaHeaderRow(multimediaFileStream);
                foreach (var filePath in multimediaFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(multimediaFileStream);
                    readStream.Close();
                }
                multimediaFileStream.Close();
            }

            // Create meta.xml
            await using var metaFileStream = archive.CreateEntry("meta.xml", CompressionLevel.Fastest).Open();
            DwcArchiveMetaFileWriter.CreateEventMetaXmlFile(metaFileStream, fieldDescriptions.ToList(), dwcExtensions, eventCoreOccurrenceFieldDescriptions);
            metaFileStream.Close();
            // Create eml.xml
            var emlFile = await _dataProviderRepository.GetEmlAsync(dataProvider.Id);
            if (emlFile == null)
            {
                throw new Exception($"No eml found for provider: {dataProvider.Identifier}");
            }
            else
            {
                DwCArchiveEmlFileFactory.SetPubDateToCurrentDate(emlFile);
                await using var emlFileStream = archive.CreateEntry("eml.xml", CompressionLevel.Fastest).Open();
                await emlFile.SaveAsync(emlFileStream, SaveOptions.None, CancellationToken.None);
                emlFileStream.Close();
            }
        }

        private ICollection<string> GetFilePaths(IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos, string searchPattern)
        {
            var filePaths = new List<string>();
            foreach (var dwcaFilePartsInfo in dwcaFilePartsInfos)
            {
                filePaths.AddRange(Directory.EnumerateFiles(
                    dwcaFilePartsInfo.ExportFolder,
                    searchPattern,
                    SearchOption.TopDirectoryOnly));
            }

            return filePaths;
        }

        private async Task WriteEventHeaderRow(CsvFileHelper csvFileHelper)
        {
            _dwcArchiveOccurrenceCsvWriter.WriteHeaderRow(csvFileHelper,
                FieldDescriptionHelper.GetAllDwcEventCoreFieldDescriptions());
            await csvFileHelper.FlushAsync();
        }

        private void WriteEventOccurrenceHeaderRow(Stream compressedFileStream)
        {
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(compressedFileStream, "\t");

            _dwcArchiveOccurrenceCsvWriter.WriteHeaderRow(csvFileHelper,
                FieldDescriptionHelper.GetAllDwcEventCoreOccurrenceFieldDescriptions());
            csvFileHelper.FinishWrite();
        }

        private void WriteEmofHeaderRow(Stream compressedFileStream, bool isEventCore = false)
        {
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(compressedFileStream, "\t");

            _extendedMeasurementOrFactCsvWriter.WriteHeaderRow(csvFileHelper, isEventCore);
            csvFileHelper.FinishWrite();
        }

        private async Task WriteMultimediaHeaderRow(Stream compressedFileStream)
        {
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(compressedFileStream, "\t");

            _simpleMultimediaCsvWriter.WriteHeaderRow(csvFileHelper);
            csvFileHelper.FinishWrite();
        }
    }
}