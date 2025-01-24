﻿using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Lib.IO.DwcArchive
{
    public class DwcArchiveEventFileWriter : IDwcArchiveEventFileWriter
    {
        private readonly IDwcArchiveOccurrenceCsvWriter _dwcArchiveOccurrenceCsvWriter;
        private readonly IDwcArchiveEventCsvWriter _dwcArchiveEventCsvWriter;
        private readonly IExtendedMeasurementOrFactCsvWriter _extendedMeasurementOrFactCsvWriter;
        private readonly IFileService _fileService;
        private readonly ISimpleMultimediaCsvWriter _simpleMultimediaCsvWriter;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly ILogger<DwcArchiveEventFileWriter> _logger;
        private readonly object _eventsLock = new object();

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

        private List<SimpleMultimediaRow> GetNonWrittenMultimedia(
           IEnumerable<SimpleMultimediaRow> multimediaRows,
           HashSet<string> writtenEventsMultimedia)
        {
            var nonWrittenMultimedia = new List<SimpleMultimediaRow>();
            lock (_eventsLock)
            {
                foreach (var row in multimediaRows)
                {
                    string key = $"{row.EventId}-{row.OccurrenceId}-{row.Identifier}";
                    if (!writtenEventsMultimedia.Contains(key))
                    {
                        writtenEventsMultimedia.Add(key);
                        nonWrittenMultimedia.Add(row);
                    }
                }
            }

            return nonWrittenMultimedia;
        }

        private async Task CreateEventDwcArchiveFileAsync(DataProvider dataProvider,
           IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos, string tempFilePath)
        {
            if (dataProvider.UseVerbatimFileInExport)
            {
                return;
            }

            using var archive = ZipFile.Open(tempFilePath, ZipArchiveMode.Create);
            var dwcExtensions = new List<DwcaEventFilePart>();

            // Create event.txt
            var eventFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-event*");
            await using var eventFileStream = archive.CreateEntry("event.txt", CompressionLevel.Fastest).Open();

            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(eventFileStream, "\t");

            await WriteEventHeaderRow(csvFileHelper);
            await csvFileHelper.FlushAsync();
            foreach (var filePath in eventFilePaths)
            {
                await using var readStream = File.OpenRead(filePath);
                await readStream.CopyToAsync(eventFileStream);
                readStream.Close();
            }
            csvFileHelper.FinishWrite();
            eventFileStream.Close();

            // Create occurrence.txt
            var eventCoreOccurrenceFieldDescriptions =
                FieldDescriptionHelper.GetAllDwcCoreEventOccurrenceFieldDescriptions().ToList();
            var occurrenceFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-occurrence*");
            if (occurrenceFilePaths.Any())
            {
                dwcExtensions.Add(DwcaEventFilePart.Occurrence);
                await using var occurrenceFileStream = archive.CreateEntry("occurrence.txt", CompressionLevel.Optimal).Open();
                csvFileHelper.InitializeWrite(occurrenceFileStream, "\t");
                _dwcArchiveOccurrenceCsvWriter.WriteHeaderRow(csvFileHelper, eventCoreOccurrenceFieldDescriptions);
                await csvFileHelper.FlushAsync();
                foreach (var filePath in occurrenceFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(occurrenceFileStream);
                    readStream.Close();
                }
                csvFileHelper.FinishWrite();
                occurrenceFileStream.Close();
            }

            // Create emof.txt
            var emofFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-emof*");
            if (emofFilePaths.Any())
            {
                dwcExtensions.Add(DwcaEventFilePart.Emof);
                await using var extendedMeasurementOrFactFileStream = archive.CreateEntry("extendedMeasurementOrFact.txt", CompressionLevel.Fastest).Open();
                csvFileHelper.InitializeWrite(extendedMeasurementOrFactFileStream, "\t");
                _extendedMeasurementOrFactCsvWriter.WriteHeaderRow(csvFileHelper, true);
                await csvFileHelper.FlushAsync();
                foreach (var filePath in emofFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(extendedMeasurementOrFactFileStream);
                    readStream.Close();
                }
                csvFileHelper.FinishWrite();
                extendedMeasurementOrFactFileStream.Close();
            }

            // Create multimedia.txt
            var multimediaFilePaths = GetFilePaths(dwcaFilePartsInfos, "event-multimedia*");
            if (multimediaFilePaths.Any())
            {
                dwcExtensions.Add(DwcaEventFilePart.Multimedia);
                await using var multimediaFileStream = archive.CreateEntry("multimedia.txt", CompressionLevel.Fastest).Open();
                csvFileHelper.InitializeWrite(multimediaFileStream, "\t");
                _simpleMultimediaCsvWriter.WriteHeaderRow(csvFileHelper, true);
                await csvFileHelper.FlushAsync();
                foreach (var filePath in multimediaFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(multimediaFileStream);
                    readStream.Close();
                }
                csvFileHelper.FinishWrite();
                multimediaFileStream.Close();
            }

            // Create meta.xml
            var fieldDescriptions = FieldDescriptionHelper.GetAllDwcEventCoreFieldDescriptions().ToList();
            await using var metaFileStream = archive.CreateEntry("meta.xml", CompressionLevel.Optimal).Open();
            DwcArchiveMetaFileWriter.CreateEventMetaXmlFile(metaFileStream, fieldDescriptions, dwcExtensions, eventCoreOccurrenceFieldDescriptions);
            metaFileStream.Close();
            // Create eml.xml
            var emlFile = await _dataProviderRepository.GetEmlAsync(dataProvider.Id);
            if (emlFile == null)
            {
                _logger.LogWarning($"No eml found for provider: {dataProvider.Identifier}");
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

        private void ValidateObservations(IEnumerable<DarwinCore> dwcObservations)
        {
            foreach (var dwcObservation in dwcObservations)
            {
                string validation = DwcaFileValidator.Validate(dwcObservation);
                if (validation != null)
                {
                    _logger.LogInformation(validation);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcArchiveOccurrenceCsvWriter"></param>
        /// <param name="dwcArchiveEventCsvWriter"></param>
        /// <param name="extendedMeasurementOrFactCsvWriter"></param>
        /// <param name="simpleMultimediaCsvWriter"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcArchiveEventFileWriter(
            IDwcArchiveOccurrenceCsvWriter dwcArchiveOccurrenceCsvWriter,
            IDwcArchiveEventCsvWriter dwcArchiveEventCsvWriter,
            IExtendedMeasurementOrFactCsvWriter extendedMeasurementOrFactCsvWriter,
            ISimpleMultimediaCsvWriter simpleMultimediaCsvWriter,
            IDataProviderRepository dataProviderRepository,
            IFileService fileService,
            ILogger<DwcArchiveEventFileWriter> logger)
        {
            _dwcArchiveOccurrenceCsvWriter = dwcArchiveOccurrenceCsvWriter ??
                                             throw new ArgumentNullException(nameof(dwcArchiveOccurrenceCsvWriter));
            _dwcArchiveEventCsvWriter = dwcArchiveEventCsvWriter ??
                                        throw new ArgumentNullException(nameof(dwcArchiveEventCsvWriter));
            _extendedMeasurementOrFactCsvWriter = extendedMeasurementOrFactCsvWriter ??
                                                  throw new ArgumentNullException(
                                                      nameof(extendedMeasurementOrFactCsvWriter));
            _simpleMultimediaCsvWriter = simpleMultimediaCsvWriter ?? throw new ArgumentNullException(nameof(simpleMultimediaCsvWriter));
            _dataProviderRepository =
                dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<DwcaWriteResult> WriteHeaderlessEventDwcaFilesAsync(
            DataProvider dataProvider,
            ICollection<Observation> processedObservations,
            Dictionary<DwcaEventFilePart, string> eventFilePathByFilePart,
            WrittenEventSets writtenEventsData,
            bool checkForIllegalCharacters = false)
        {
            if (!processedObservations?.Any() ?? true)
            {
                return new DwcaWriteResult() { DataProviderIdentifier = dataProvider.Identifier };
            }
            int eventsCount = 0;
            int occurrenceCount = 0;
            int emofCount = 0;
            int multimediaCount = 0;

            // Create Event CSV file
            var nonProcessedObservationEvents = GetNonProcessedObservationEvents(processedObservations, writtenEventsData.WrittenEvents);
            var eventCsvFilePath = eventFilePathByFilePart[DwcaEventFilePart.Event];
            bool fixSbdiArtportalenInstitutionCode = dataProvider.Id == 1;
            var dwcObservations = nonProcessedObservationEvents.ToDarwinCore(fixSbdiArtportalenInstitutionCode);
            if (dwcObservations != null && dwcObservations.Any())
            {
                if (checkForIllegalCharacters)
                {
                    ValidateObservations(dwcObservations);
                }
                eventsCount = dwcObservations.Count();
                var eventFieldDescriptions = FieldDescriptionHelper.GetAllDwcEventCoreFieldDescriptions();
                await using var eventFileStream = File.AppendText(eventCsvFilePath);
                await _dwcArchiveEventCsvWriter.WriteHeaderlessEventCsvFileAsync(
                    dwcObservations,
                    eventFileStream,
                    eventFieldDescriptions);
            }

            // Create Occurrence event CSV file - exclude event data
            var occurrenceCsvFilePath = eventFilePathByFilePart[DwcaEventFilePart.Occurrence];
            var dwcEventObservations = processedObservations.ToDarwinCore(fixSbdiArtportalenInstitutionCode);
            if (dwcEventObservations != null && dwcEventObservations.Any())
            {
                occurrenceCount = dwcEventObservations.Count();
                var occurrenceFieldDescriptions = FieldDescriptionHelper.GetAllDwcCoreEventOccurrenceFieldDescriptions();
                await using StreamWriter occurrenceFileStream = File.AppendText(occurrenceCsvFilePath);
                await _dwcArchiveOccurrenceCsvWriter.WriteHeaderlessOccurrenceCsvFileAsync(
                    dwcEventObservations,
                    occurrenceFileStream,
                    occurrenceFieldDescriptions,
                    true);
            }

            // Create EMOF Event CSV file
            var emofCsvFilePath = eventFilePathByFilePart[DwcaEventFilePart.Emof];
            var emofRows = processedObservations.ToExtendedMeasurementOrFactRows(true);
            emofRows = GetNonWrittenMeasurements(emofRows, writtenEventsData.WrittenMeasurements);
            if (emofRows != null && emofRows.Any())
            {
                emofCount = emofRows.Count();
                await using StreamWriter emofFileStream = File.AppendText(emofCsvFilePath);
                await _extendedMeasurementOrFactCsvWriter.WriteHeaderlessEmofCsvFileAsync(
                    emofRows,
                    emofFileStream,
                    true);
            }

            // Create Multimedia CSV file
            string multimediaCsvFilePath = eventFilePathByFilePart[DwcaEventFilePart.Multimedia];
            var multimediaRows = processedObservations.ToSimpleMultimediaRows();
            multimediaRows = GetNonWrittenMultimedia(multimediaRows, writtenEventsData.WrittenMultimedia);
            if (multimediaRows != null && multimediaRows.Any())
            {
                multimediaCount = multimediaRows.Count();
                await using StreamWriter multimediaFileStream = File.AppendText(multimediaCsvFilePath);
                _simpleMultimediaCsvWriter.WriteHeaderlessCsvFile(
                    multimediaRows,
                    multimediaFileStream,
                    true);
            }

            return new DwcaWriteResult
            {
                DataProviderIdentifier = dataProvider.Identifier,
                EventCount = eventsCount,
                OccurrenceCount = occurrenceCount,
                EmofCount = emofCount,
                MultimediaCount = multimediaCount
            };
        }

        public async Task<FileExportResult> CreateEventDwcArchiveFileAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            string fileName,
            IProcessedObservationCoreRepository processedObservationRepository,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken)
        {
            var temporaryZipExportFolderPath = Path.Combine(exportFolderPath, "zip");

            try
            {
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                var eventCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "event.txt");
                var occurrenceCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "occurrence.txt");
                var emofCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "extendedMeasurementOrFact.txt");
                var multimediaCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "multimedia.txt");
                var metaXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "meta.xml");
                var emlXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "eml.xml");
                var processInfoXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "processinfo.xml");
                bool eventFileCreated = false;
                bool emofFileCreated = false;
                bool multimediaFileCreated = false;
                int nrObservations = 0;
                var expectedNoOfObservations = await processedObservationRepository.GetMatchCountAsync(filter);

                // Create Event.txt
                var eventFieldDescriptions = FieldDescriptionHelper.GetAllDwcEventCoreFieldDescriptions().ToList();
                using (var fileStream = File.Create(eventCsvFilePath, 128 * 1024))
                {
                    eventFileCreated = await _dwcArchiveEventCsvWriter.CreateEventCsvFileAsync(
                        filter,
                        fileStream,
                        eventFieldDescriptions,
                        processedObservationRepository,
                        cancellationToken);
                }

                // Create Occurrence.txt
                var occurrenceFieldDescriptions =
                    FieldDescriptionHelper.GetAllDwcCoreEventOccurrenceFieldDescriptions().ToList();
                using (var fileStream = File.Create(occurrenceCsvFilePath, 128 * 1024))
                {
                    nrObservations = await _dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                        filter,
                        fileStream,
                        occurrenceFieldDescriptions,
                        processedObservationRepository,
                        cancellationToken,
                        false,
                        true);
                }

                // If less than 99% of expected observations where fetched, something is wrong
                if (nrObservations < expectedNoOfObservations * 0.99)
                {
                    throw new Exception($"Dwc export expected {expectedNoOfObservations} but only got {nrObservations}");
                }

                // Create ExtendedMeasurementOrFact.txt
                using (var fileStream = File.Create(emofCsvFilePath))
                {
                    emofFileCreated = await _extendedMeasurementOrFactCsvWriter.CreateCsvFileAsync(
                        filter,
                        fileStream,
                        processedObservationRepository,
                        cancellationToken,
                        true);
                }

                // Create multimedia.txt
                using (var fileStream = File.Create(multimediaCsvFilePath))
                {
                    multimediaFileCreated = await _simpleMultimediaCsvWriter.CreateCsvFileAsync(
                        filter,
                        fileStream,
                        processedObservationRepository,
                        cancellationToken,
                        true);
                }

                // Delete extension files if not used.
                if (!emofFileCreated && File.Exists(emofCsvFilePath)) File.Delete(emofCsvFilePath);
                if (!multimediaFileCreated && File.Exists(multimediaCsvFilePath)) File.Delete(multimediaCsvFilePath);

                // Create meta.xml
                using (var fileStream = File.Create(metaXmlFilePath))
                {
                    var dwcExtensions = new List<DwcaEventFilePart>();
                    dwcExtensions.Add(DwcaEventFilePart.Occurrence);
                    if (emofFileCreated) dwcExtensions.Add(DwcaEventFilePart.Emof);
                    if (multimediaFileCreated) dwcExtensions.Add(DwcaEventFilePart.Multimedia);
                    DwcArchiveMetaFileWriter.CreateEventMetaXmlFile(fileStream, eventFieldDescriptions, dwcExtensions, occurrenceFieldDescriptions);
                }

                var emlFile = await _dataProviderRepository.GetEmlAsync(dataProvider.Id);
                if (emlFile == null)
                {
                    throw new Exception($"No eml found for provider: {dataProvider.Identifier}");
                }
                else
                {
                    DwCArchiveEmlFileFactory.SetPubDateToCurrentDate(emlFile);
                    await using var fileStream = File.Create(emlXmlFilePath);
                    await emlFile.SaveAsync(fileStream, SaveOptions.None, CancellationToken.None);
                }

                // Create processinfo.xml
                if (processInfo != null)
                {
                    await using var processInfoFileStream = File.Create(processInfoXmlFilePath);
                    DwcProcessInfoFileWriter.CreateProcessInfoFile(processInfoFileStream, processInfo);
                    processInfoFileStream.Close();
                }
                var zipFilePath = Path.Join(exportFolderPath, $"{fileName}.zip");
                _fileService.CompressDirectory(temporaryZipExportFolderPath, zipFilePath);
                return new FileExportResult
                {
                    FilePath = zipFilePath,
                    NrObservations = nrObservations
                };
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("CreateDwcArchiveFile was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Dwc Archive File.");
                throw;
            }
            finally
            {
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }

        public async Task<string> CreateEventDwcArchiveFileAsync(
            DataProvider dataProvider,
            string exportFolderPath,
            DwcaFilePartsInfo dwcaFilePartsInfo)
        {
            string tempFilePath = null;
            try
            {
                tempFilePath = Path.Combine(exportFolderPath, $"Temp_{Path.GetRandomFileName()}.event.dwca.zip");
                var filePath = Path.Combine(exportFolderPath, $"{dwcaFilePartsInfo.DataProvider.Identifier}-event.dwca.zip");

                // Create the DwC-A file
                await CreateEventDwcArchiveFileAsync(dataProvider, new[] { dwcaFilePartsInfo }, tempFilePath);

                _fileService.MoveFile(tempFilePath, filePath);
                _logger.LogInformation($"A new .zip({filePath}) was created. " + "DataProvider={@dataProvider}", dataProvider?.Identifier);
                return filePath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Creating DwC-A .zip for {@dataProvider} failed", dwcaFilePartsInfo?.DataProvider?.Identifier);
                throw;
            }
            finally
            {
                _fileService.DeleteFile(tempFilePath);
            }
        }

        public async Task<(Stream stream, string filename)> CreateEventDwcArchiveFileInMemoryAsync(
            DataProvider dataProvider, 
            SearchFilter filter, 
            IProcessedObservationCoreRepository processedObservationRepository, 
            ProcessInfo processInfo, 
            IJobCancellationToken cancellationToken)
        {
            var memoryStream = new MemoryStream();

            try
            {
                bool eventFileCreated = false;
                bool emofFileCreated = false;
                bool multimediaFileCreated = false;
                int nrObservations = 0;
                string fileName = $"Observations {DateTime.Now.ToString("yyyy-MM-dd HH.mm")} SOS export";
                var expectedNoOfObservations = await processedObservationRepository.GetMatchCountAsync(filter);

                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update, leaveOpen: true))
                {
                    // Create Event.txt
                    var eventFieldDescriptions = FieldDescriptionHelper.GetAllDwcEventCoreFieldDescriptions().ToList();
                    var eventFileEntry = zipArchive.CreateEntry("event.txt", System.IO.Compression.CompressionLevel.Optimal);
                    using (var eventFileZipStream = eventFileEntry.Open())
                    {
                        eventFileCreated = await _dwcArchiveEventCsvWriter.CreateEventCsvFileAsync(
                            filter,
                            eventFileZipStream,
                            eventFieldDescriptions,
                            processedObservationRepository,
                            cancellationToken);
                    }

                    // Create Occurrence.txt
                    var occurrenceFieldDescriptions = FieldDescriptionHelper.GetAllDwcCoreEventOccurrenceFieldDescriptions().ToList();
                    var occurrenceFileEntry = zipArchive.CreateEntry("occurrence.txt", System.IO.Compression.CompressionLevel.Optimal);
                    using (var occurrenceFileZipStream = occurrenceFileEntry.Open())
                    {
                        nrObservations = await _dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                            filter,
                            occurrenceFileZipStream,
                            occurrenceFieldDescriptions,
                            processedObservationRepository,
                            cancellationToken,
                            false,
                            true);                        
                    }

                    // If less than 99% of expected observations where fetched, something is wrong
                    if (nrObservations < expectedNoOfObservations * 0.99)
                    {
                        throw new Exception($"Dwc export expected {expectedNoOfObservations} but only got {nrObservations}");
                    }

                    // Create ExtendedMeasurementOrFact.txt
                    var emofFileEntry = zipArchive.CreateEntry("extendedMeasurementOrFact.txt", System.IO.Compression.CompressionLevel.Optimal);
                    using (var emofFileZipStream = emofFileEntry.Open())
                    {                     
                        emofFileCreated = await _extendedMeasurementOrFactCsvWriter.CreateCsvFileAsync(
                            filter,
                            emofFileZipStream,
                            processedObservationRepository,
                            cancellationToken,
                            true);                        
                    }
                    if (!emofFileCreated) emofFileEntry.Delete(); // Delete extension files if not used.

                    // Create multimedia.txt
                    var multimediaFileEntry = zipArchive.CreateEntry("multimedia.txt", System.IO.Compression.CompressionLevel.Optimal);
                    using (var multimediaFileZipStream = multimediaFileEntry.Open())
                    { 
                        multimediaFileCreated = await _simpleMultimediaCsvWriter.CreateCsvFileAsync(
                            filter,
                            multimediaFileZipStream,
                            processedObservationRepository,
                            cancellationToken,
                            true);
                    }
                    if (!multimediaFileCreated) multimediaFileEntry.Delete(); // Delete extension files if not used.

                    // Create meta.xml
                    var metaFileEntry = zipArchive.CreateEntry("meta.xml", System.IO.Compression.CompressionLevel.Optimal);
                    using (var metaFileZipStream = metaFileEntry.Open())
                    {  
                        var dwcExtensions = new List<DwcaEventFilePart>();
                        dwcExtensions.Add(DwcaEventFilePart.Occurrence);
                        if (emofFileCreated) dwcExtensions.Add(DwcaEventFilePart.Emof);
                        if (multimediaFileCreated) dwcExtensions.Add(DwcaEventFilePart.Multimedia);
                        DwcArchiveMetaFileWriter.CreateEventMetaXmlFile(metaFileZipStream, eventFieldDescriptions, dwcExtensions, occurrenceFieldDescriptions);
                    }

                    var emlFile = await _dataProviderRepository.GetEmlAsync(dataProvider.Id);
                    if (emlFile == null)
                    {
                        throw new Exception($"No eml found for provider: {dataProvider.Identifier}");
                    }
                    else
                    {
                        DwCArchiveEmlFileFactory.SetPubDateToCurrentDate(emlFile);
                        var emlFileEntry = zipArchive.CreateEntry("eml.xml", System.IO.Compression.CompressionLevel.Optimal);
                        using (var emlFileZipStream = emlFileEntry.Open())
                        {
                            await emlFile.SaveAsync(emlFileZipStream, SaveOptions.None, CancellationToken.None);
                        }
                    }

                    // Create processinfo.xml
                    if (processInfo != null)
                    {
                        var processInfoFileEntry = zipArchive.CreateEntry("processinfo.xml", System.IO.Compression.CompressionLevel.Optimal);
                        using (var processInfoFileZipStream = processInfoFileEntry.Open())
                        {
                            DwcProcessInfoFileWriter.CreateProcessInfoFile(processInfoFileZipStream, processInfo);
                        }
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return (memoryStream, fileName);
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("CreateDwcArchiveFile was canceled. DataProvider={@dataProvider}", dataProvider.Identifier);
                if (memoryStream != null) memoryStream.Dispose();
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Dwc Archive File. DataProvider={@dataProvider}", dataProvider.Identifier);
                if (memoryStream != null) memoryStream.Dispose();
                throw;
            }
        }
    }
}