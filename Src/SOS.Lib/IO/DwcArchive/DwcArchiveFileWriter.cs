﻿using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Models;
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
    public class DwcArchiveFileWriter : FileWriterBase, IDwcArchiveFileWriter
    {
        private readonly IDwcArchiveOccurrenceCsvWriter _dwcArchiveOccurrenceCsvWriter;
        private readonly IExtendedMeasurementOrFactCsvWriter _extendedMeasurementOrFactCsvWriter;
        private readonly ISimpleMultimediaCsvWriter _simpleMultimediaCsvWriter;
        private readonly IFileService _fileService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly ILogger<DwcArchiveFileWriter> _logger;


        private async Task CreateDwcArchiveFileAsync(DataProvider dataProvider, DwcaFilePartsInfo dwcaFilePartsInfo,
            string tempFilePath)
        {
            await CreateDwcArchiveFileAsync(dataProvider, new[] { dwcaFilePartsInfo }, tempFilePath);
        }

        private ICollection<string> GetFilePaths(IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos, string searchPattern)
        {
            var filePaths = new List<string>();
            foreach (var dwcaFilePartsInfo in dwcaFilePartsInfos)
            {
                if (Directory.Exists(dwcaFilePartsInfo.ExportFolder))
                {
                    filePaths.AddRange(Directory.EnumerateFiles(
                    dwcaFilePartsInfo.ExportFolder,
                    searchPattern,
                    SearchOption.TopDirectoryOnly));
                }
            }

            return filePaths;
        }

        private void ValidateObservations(IEnumerable<DarwinCore> dwcObservations)
        {
            if (dwcObservations == null) return;
            foreach (var dwcObservation in dwcObservations)
            {
                string validation = DwcaFileValidator.Validate(dwcObservation);
                if (validation != null)
                {
                    _logger.LogInformation(validation);
                }
            }
        }

        private void ValidateEmofRows(IEnumerable<ExtendedMeasurementOrFactRow> emofRows)
        {
            foreach (var emofRow in emofRows)
            {
                string validation = DwcaFileValidator.Validate(emofRow);
                if (validation != null)
                {
                    _logger.LogInformation(validation);
                }
            }
        }

        private void ValidateMultimediaRows(IEnumerable<SimpleMultimediaRow> multimediaRows)
        {
            foreach (var multimediaRow in multimediaRows)
            {
                string validation = DwcaFileValidator.Validate(multimediaRow);
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
        /// <param name="extendedMeasurementOrFactCsvWriter"></param>
        /// <param name="simpleMultimediaCsvWriter"></param>
        /// <param name="fileService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcArchiveFileWriter(IDwcArchiveOccurrenceCsvWriter dwcArchiveOccurrenceCsvWriter,
            IExtendedMeasurementOrFactCsvWriter extendedMeasurementOrFactCsvWriter,
            ISimpleMultimediaCsvWriter simpleMultimediaCsvWriter,
            IFileService fileService,
            IDataProviderRepository dataProviderRepository,
            ILogger<DwcArchiveFileWriter> logger)
        {
            _dwcArchiveOccurrenceCsvWriter = dwcArchiveOccurrenceCsvWriter ??
                                             throw new ArgumentNullException(nameof(dwcArchiveOccurrenceCsvWriter));
            _extendedMeasurementOrFactCsvWriter = extendedMeasurementOrFactCsvWriter ??
                                                  throw new ArgumentNullException(
                                                      nameof(extendedMeasurementOrFactCsvWriter));
            _simpleMultimediaCsvWriter = simpleMultimediaCsvWriter ?? throw new ArgumentNullException(nameof(simpleMultimediaCsvWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _dataProviderRepository =
                dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<FileExportResult> CreateDwcArchiveFileAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            string fileName,
            IProcessedObservationCoreRepository processedObservationRepository,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken)
        {
            IEnumerable<FieldDescription> fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();

            return await CreateDwcArchiveFileAsync(
                dataProvider,
                filter,
                fileName,
                processedObservationRepository,
                fieldDescriptions,
                processInfo,
                exportFolderPath,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<FileExportResult> CreateDwcArchiveFileAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            string fileName,
            IProcessedObservationCoreRepository processedObservationRepository,
            IEnumerable<FieldDescription> fieldDescriptions,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken)
        {
            var temporaryZipExportFolderPath = Path.Combine(exportFolderPath, "zip");

            try
            {
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                var occurrenceCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "occurrence.txt");
                var emofCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "extendedMeasurementOrFact.txt");
                var multimediaCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "multimedia.txt");
                var metaXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "meta.xml");
                var emlXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "eml.xml");
                var processInfoXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "processinfo.xml");
                bool emofFileCreated = false;
                bool multimediaFileCreated = false;
                int nrObservations = 0;
                var expectedNoOfObservations = await processedObservationRepository.GetMatchCountAsync(filter);

                // Create Occurrence.txt
                using (var fileStream = File.Create(occurrenceCsvFilePath, 128 * 1024))
                {
                    nrObservations = await _dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                        filter,
                        fileStream,
                        fieldDescriptions,
                        processedObservationRepository,
                        cancellationToken);
                }

                // If less tha 99% of expected observations where fetched, something is wrong
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
                        cancellationToken);
                }

                // Create multimedia.txt
                using (var fileStream = File.Create(multimediaCsvFilePath))
                {
                    multimediaFileCreated = await _simpleMultimediaCsvWriter.CreateCsvFileAsync(
                        filter,
                        fileStream,
                        processedObservationRepository,
                        cancellationToken);
                }

                // Delete extension files if not used.
                if (!emofFileCreated && File.Exists(emofCsvFilePath)) _fileService.DeleteFile(emofCsvFilePath);
                if (!multimediaFileCreated && File.Exists(multimediaCsvFilePath)) _fileService.DeleteFile(multimediaCsvFilePath);

                // Create meta.xml
                using (var fileStream = File.Create(metaXmlFilePath))
                {
                    var dwcExtensions = new List<DwcaFilePart>();
                    if (emofFileCreated) dwcExtensions.Add(DwcaFilePart.Emof);
                    if (multimediaFileCreated) dwcExtensions.Add(DwcaFilePart.Multimedia);
                    DwcArchiveMetaFileWriter.CreateMetaXmlFile(fileStream, fieldDescriptions.ToList(), dwcExtensions);
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
                _logger.LogInformation("CreateDwcArchiveFile was canceled. DataProvider={@dataProvider}", dataProvider.Identifier);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Dwc Archive File. DataProvider={@dataProvider}", dataProvider.Identifier);
                throw;
            }
            finally
            {
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }

        public async Task<(Stream stream, string filename)> CreateDwcArchiveFileInMemoryAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            IProcessedObservationCoreRepository processedObservationRepository,
            IEnumerable<FieldDescription> fieldDescriptions,
            ProcessInfo processInfo,
            IJobCancellationToken cancellationToken)
        {            
            var memoryStream = new MemoryStream();

            try
            {                
                bool emofFileCreated = false;
                bool multimediaFileCreated = false;
                int nrObservations = 0;
                string fileName = $"Observations {DateTime.Now.ToString("yyyy-MM-dd HH.mm")} SOS export";
                var expectedNoOfObservations = await processedObservationRepository.GetMatchCountAsync(filter);
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update, leaveOpen: true))
                {
                    // Create Occurrence.txt
                    var occurrenceFileEntry = zipArchive.CreateEntry("occurrence.txt", System.IO.Compression.CompressionLevel.Optimal);
                    using (var occurrenceFileZipStream = occurrenceFileEntry.Open())
                    {
                        nrObservations = await _dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                            filter,
                            occurrenceFileZipStream,
                            fieldDescriptions,
                            processedObservationRepository,
                            cancellationToken);                        
                    }

                    // If less tha 99% of expected observations where fetched, something is wrong
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
                            cancellationToken);
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
                            cancellationToken);
                    }
                    if (!multimediaFileCreated) multimediaFileEntry.Delete(); // Delete extension files if not used.
                   
                    // Create meta.xml
                    var metaFileEntry = zipArchive.CreateEntry("meta.xml", System.IO.Compression.CompressionLevel.Optimal);
                    using (var metaFileZipStream = metaFileEntry.Open())
                    {
                        var dwcExtensions = new List<DwcaFilePart>();
                        if (emofFileCreated) dwcExtensions.Add(DwcaFilePart.Emof);
                        if (multimediaFileCreated) dwcExtensions.Add(DwcaFilePart.Multimedia);
                        DwcArchiveMetaFileWriter.CreateMetaXmlFile(metaFileZipStream, fieldDescriptions.ToList(), dwcExtensions);
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

        public async Task<DwcaWriteResult> WriteHeaderlessDwcaFiles(
            DataProvider dataProvider,
            ICollection<Observation> processedObservations,
            Dictionary<DwcaFilePart, string> filePathByFilePart,
            DwcaFilePartsInfo dwcaFilePartsInfo,
            bool checkForIllegalCharacters = false)
        {
            if (!processedObservations?.Any() ?? true)
            {
                return new DwcaWriteResult() { DataProviderIdentifier = dataProvider.Identifier };
            }
            int occurrenceCount = 0;
            int emofCount = 0;
            int multimediaCount = 0;
            var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();

            // Create Occurrence txt file
            string occurrenceCsvFilePath = filePathByFilePart[DwcaFilePart.Occurrence];
            bool fixSbdiArtportalenInstitutionCode = dataProvider.Id == 1;
            var dwcObservations = processedObservations.ToDarwinCore(fixSbdiArtportalenInstitutionCode);
            if (checkForIllegalCharacters) ValidateObservations(dwcObservations);
            if (dwcObservations != null && dwcObservations.Any())
            {
                occurrenceCount = dwcObservations.Count();
                await using StreamWriter occurrenceFileStream = File.AppendText(occurrenceCsvFilePath);
                await _dwcArchiveOccurrenceCsvWriter.WriteHeaderlessOccurrenceCsvFileAsync(
                    dwcObservations,
                    occurrenceFileStream,
                    fieldDescriptions);
            }
            // Create EMOF txt file
            string emofCsvFilePath = filePathByFilePart[DwcaFilePart.Emof];
            var emofRows = processedObservations.ToExtendedMeasurementOrFactRows();
            if (checkForIllegalCharacters) ValidateEmofRows(emofRows);
            if (emofRows != null && emofRows.Any())
            {
                emofCount = emofRows.Count();
                await using StreamWriter emofFileStream = File.AppendText(emofCsvFilePath);
                await _extendedMeasurementOrFactCsvWriter.WriteHeaderlessEmofCsvFileAsync(
                    emofRows,
                    emofFileStream);
            }

            // Create Multimedia txt file
            string multimediaCsvFilePath = filePathByFilePart[DwcaFilePart.Multimedia];
            List<SimpleMultimediaRow> multimediaRows;
            if (dataProvider.Id == 1) // Artportalen
            {
                multimediaRows = processedObservations.ToArtportalenSimpleMultimediaRows().ToList();                
            }
            else
            {
                multimediaRows = processedObservations.ToSimpleMultimediaRows().ToList();
            }

            if (checkForIllegalCharacters) ValidateMultimediaRows(multimediaRows);
            if (multimediaRows != null && multimediaRows.Any())
            {
                multimediaCount = multimediaRows.Count();
                await using var multimediaFileStream = File.AppendText(multimediaCsvFilePath);

                _simpleMultimediaCsvWriter.WriteHeaderlessCsvFile(
                    multimediaRows,
                    multimediaFileStream);
            }

            return new DwcaWriteResult
            {
                DataProviderIdentifier = dataProvider.Identifier,
                OccurrenceCount = occurrenceCount,
                EmofCount = emofCount,
                MultimediaCount = multimediaCount,
            };
        }

        public async Task<string> CreateDwcArchiveFileAsync(
            DataProvider dataProvider,
            string exportFolderPath,
            DwcaFilePartsInfo dwcaFilePartsInfo)
        {
            string tempFilePath = null;
            try
            {
                tempFilePath = Path.Combine(exportFolderPath, $"Temp_{Path.GetRandomFileName()}.dwca.zip");
                var filePath = Path.Combine(exportFolderPath, $"{dwcaFilePartsInfo.DataProvider.Identifier}.dwca.zip");

                // Create the DwC-A file
                await CreateDwcArchiveFileAsync(dataProvider, dwcaFilePartsInfo, tempFilePath);

                _fileService.MoveFile(tempFilePath, filePath);
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
                _fileService.DeleteFile(tempFilePath);
            }
        }

        public async Task<string> CreateCompleteDwcArchiveFileAsync(string exportFolderPath, IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos)
        {
            string tempFilePath = null;
            try
            {
                tempFilePath = Path.Combine(exportFolderPath, $"Temp_{Path.GetRandomFileName()}.dwca.zip");
                var filePath = Path.Combine(exportFolderPath, "sos.dwca.zip");

                // Create the DwC-A file
                await CreateDwcArchiveFileAsync(DataProvider.CompleteSosDataProvider, dwcaFilePartsInfos, tempFilePath);

                File.Move(tempFilePath, filePath, true);
                _logger.LogInformation($"A new .zip({filePath}) was created.");

                return filePath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Creating complete DwC-A .zip failed");
                throw;
            }
            finally
            {
                _fileService.DeleteFile(tempFilePath);
            }
        }

        public async Task CreateDwcArchiveFileAsync(DataProvider dataProvider,
            IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos, string tempFilePath)
        {
            if (dataProvider.UseVerbatimFileInExport)
            {
                return;
            }

            using var archive = ZipFile.Open(tempFilePath, ZipArchiveMode.Create);
            var dwcExtensions = new List<DwcaFilePart>();

            // Create occurrence.txt
            var occurrenceFilePaths = GetFilePaths(dwcaFilePartsInfos, "occurrence*");
            await using var occurrenceFileStream = archive.CreateEntry("occurrence.txt", CompressionLevel.Optimal).Open();

            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(occurrenceFileStream, "\t");

            _dwcArchiveOccurrenceCsvWriter.WriteHeaderRow(csvFileHelper,
                FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions());
            await csvFileHelper.FlushAsync();

            foreach (var filePath in occurrenceFilePaths)
            {
                await using var readStream = File.OpenRead(filePath);
                await readStream.CopyToAsync(occurrenceFileStream);
                readStream.Close();
            }
            csvFileHelper.FinishWrite();
            occurrenceFileStream.Close();

            // Create emof.txt
            var emofFilePaths = GetFilePaths(dwcaFilePartsInfos, "emof*");
            if (emofFilePaths.Any())
            {
                dwcExtensions.Add(DwcaFilePart.Emof);
                await using var extendedMeasurementOrFactFileStream = archive.CreateEntry("extendedMeasurementOrFact.txt", CompressionLevel.Optimal).Open();
                csvFileHelper.InitializeWrite(extendedMeasurementOrFactFileStream, "\t");
                _extendedMeasurementOrFactCsvWriter.WriteHeaderRow(csvFileHelper);
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
            var multimediaFilePaths = GetFilePaths(dwcaFilePartsInfos, "multimedia*");
            if (multimediaFilePaths.Any())
            {
                dwcExtensions.Add(DwcaFilePart.Multimedia);
                await using var multimediaFileStream = archive.CreateEntry("multimedia.txt", CompressionLevel.Optimal).Open();
                csvFileHelper.InitializeWrite(multimediaFileStream, "\t");
                _simpleMultimediaCsvWriter.WriteHeaderRow(csvFileHelper, false);
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
            var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions().ToList();
            await using var metaFileStream = archive.CreateEntry("meta.xml", CompressionLevel.Optimal).Open();
            DwcArchiveMetaFileWriter.CreateMetaXmlFile(metaFileStream, fieldDescriptions.ToList(), dwcExtensions);
            metaFileStream.Close();

            var emlFile = await _dataProviderRepository.GetEmlAsync(dataProvider.Id);
            if (emlFile == null)
            {
                _logger.LogWarning($"No eml found for provider: {dataProvider.Identifier}");
            }
            else
            {
                // Create eml.xml
                DwCArchiveEmlFileFactory.SetPubDateToCurrentDate(emlFile);
                await using var enlFileStream = archive.CreateEntry("eml.xml", CompressionLevel.Optimal).Open();

                await emlFile.SaveAsync(enlFileStream, SaveOptions.None, CancellationToken.None);
                enlFileStream.Close();
            }
        }
    }
}