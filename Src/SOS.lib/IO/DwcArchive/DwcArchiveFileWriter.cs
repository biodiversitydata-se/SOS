﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Ionic.Zip;
using Microsoft.Extensions.Logging;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.IO.DwcArchive
{
    public class DwcArchiveFileWriter : IDwcArchiveFileWriter
    {
        private readonly IDwcArchiveOccurrenceCsvWriter _dwcArchiveOccurrenceCsvWriter;
        private readonly IExtendedMeasurementOrFactCsvWriter _extendedMeasurementOrFactCsvWriter;
        private readonly ISimpleMultimediaCsvWriter _simpleMultimediaCsvWriter;
        private readonly IFileService _fileService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly ILogger<DwcArchiveFileWriter> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dwcArchiveOccurrenceCsvWriter"></param>
        /// <param name="extendedMeasurementOrFactCsvWriter"></param>
        /// <param name="simpleMultimediaCsvWriter"></param>
        /// <param name="fileService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="logger"></param>
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
        public async Task<string> CreateDwcArchiveFileAsync(
            DataProvider dataProvider, 
            FilterBase filter,
            string fileName,
            IProcessedObservationRepository processedObservationRepository,
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
        public async Task<string> CreateDwcArchiveFileAsync(
            DataProvider dataProvider, 
            FilterBase filter,
            string fileName,
            IProcessedObservationRepository processedObservationRepository,
            IEnumerable<FieldDescription> fieldDescriptions,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                temporaryZipExportFolderPath = Path.Combine(exportFolderPath, fileName);
                _fileService.CreateFolder(temporaryZipExportFolderPath);
                var occurrenceCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "occurrence.csv");
                var emofCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "extendedMeasurementOrFact.csv");
                var multimediaCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "multimedia.csv");
                var metaXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "meta.xml");
                var emlXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "eml.xml");
                var processInfoXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "processinfo.xml");
                bool emofFileCreated = false;
                bool multimediaFileCreated = false;

                // Create Occurrence.csv
                using (var fileStream = File.Create(occurrenceCsvFilePath, 128 * 1024))
                {
                    await _dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                        filter,
                        fileStream,
                        fieldDescriptions,
                        processedObservationRepository,
                        cancellationToken);
                }

                // Create ExtendedMeasurementOrFact.csv
                using (var fileStream = File.Create(emofCsvFilePath))
                {
                    emofFileCreated = await _extendedMeasurementOrFactCsvWriter.CreateCsvFileAsync(
                        filter,
                        fileStream,
                        fieldDescriptions,
                        processedObservationRepository,
                        cancellationToken);
                }

                // Create multimedia.csv
                using (var fileStream = File.Create(multimediaCsvFilePath))
                {
                    multimediaFileCreated = await _simpleMultimediaCsvWriter.CreateCsvFileAsync(
                        filter,
                        fileStream,
                        fieldDescriptions,
                        processedObservationRepository,
                        cancellationToken);
                }

                // Delete extension files if not used.
                if (!emofFileCreated && File.Exists(emofCsvFilePath)) File.Delete(emofCsvFilePath);
                if (!multimediaFileCreated && File.Exists(multimediaCsvFilePath)) File.Delete(multimediaCsvFilePath);

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
                    _logger.LogWarning($"No eml found for provider: {dataProvider.Identifier}");
                }
                else
                {
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
               
                var zipFilePath = _fileService.CompressFolder(exportFolderPath, fileName);
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
                return zipFilePath;
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
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
        }
        
        public async Task WriteHeaderlessDwcaFiles(
            ICollection<Observation> processedObservations,
            Dictionary<DwcaFilePart, string> filePathByFilePart)
        {
            if (!processedObservations?.Any() ?? true)
            {
                return;
            }

            var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();

            // Create Occurrence CSV file
            string occurrenceCsvFilePath = filePathByFilePart[DwcaFilePart.Occurrence];
            var dwcObservations = processedObservations.ToDarwinCore();
            await using StreamWriter occurrenceFileStream = File.AppendText(occurrenceCsvFilePath);
            await _dwcArchiveOccurrenceCsvWriter.WriteHeaderlessOccurrenceCsvFileAsync(
                dwcObservations,
                occurrenceFileStream,
                fieldDescriptions);

            // Create EMOF CSV file
            string emofCsvFilePath = filePathByFilePart[DwcaFilePart.Emof];
            var emofRows = processedObservations.ToExtendedMeasurementOrFactRows();
            if (emofRows != null && emofRows.Any())
            {
                await using StreamWriter emofFileStream = File.AppendText(emofCsvFilePath);
                await _extendedMeasurementOrFactCsvWriter.WriteHeaderlessEmofCsvFileAsync(
                    emofRows,
                    emofFileStream);
            }

            // Create Multimedia CSV file
            string multimediaCsvFilePath = filePathByFilePart[DwcaFilePart.Multimedia];
            var multimediaRows = processedObservations.ToSimpleMultimediaRows();
            if (multimediaRows != null && multimediaRows.Any())
            {
                await using StreamWriter multimediaFileStream = File.AppendText(multimediaCsvFilePath);
                await _simpleMultimediaCsvWriter.WriteHeaderlessCsvFileAsync(
                    multimediaRows,
                    multimediaFileStream);
            }
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
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
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
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        private async Task CreateDwcArchiveFileAsync(DataProvider dataProvider, DwcaFilePartsInfo dwcaFilePartsInfo,
            string tempFilePath)
        {
            await CreateDwcArchiveFileAsync(dataProvider, new[] { dwcaFilePartsInfo }, tempFilePath);
        }

        private async Task CreateDwcArchiveFileAsync(DataProvider dataProvider,
            IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos, string tempFilePath)
        {
            if (dataProvider.UseVerbatimFileInExport)
            {
                return;
            }

            var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions().ToList();
            await using var stream = File.Create(tempFilePath);
            await using var compressedFileStream = new ZipOutputStream(stream, true) { EnableZip64 = Zip64Option.AsNecessary };

            // Create meta.xml
            var dwcExtensions = new List<DwcaFilePart>();
            
            // Create occurrence.csv
            var occurrenceFilePaths = GetFilePaths(dwcaFilePartsInfos, "occurrence*");
            compressedFileStream.PutNextEntry("occurrence.csv");
            await WriteOccurrenceHeaderRow(compressedFileStream);
            foreach (var filePath in occurrenceFilePaths)
            {
                await using var readStream = File.OpenRead(filePath);
                await readStream.CopyToAsync(compressedFileStream);
            }
            
            // Create emof.csv
            var emofFilePaths = GetFilePaths(dwcaFilePartsInfos, "emof*");
            if (emofFilePaths.Any())
            {
                dwcExtensions.Add(DwcaFilePart.Emof);
                compressedFileStream.PutNextEntry("extendedMeasurementOrFact.csv");
                await WriteEmofHeaderRow(compressedFileStream);
                foreach (var filePath in emofFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(compressedFileStream);
                }
            }
            
            // Create multimedia.csv
            var multimediaFilePaths = GetFilePaths(dwcaFilePartsInfos, "multimedia*");
            if (multimediaFilePaths.Any())
            {
                dwcExtensions.Add(DwcaFilePart.Multimedia);
                compressedFileStream.PutNextEntry("multimedia.csv");
                await WriteMultimediaHeaderRow(compressedFileStream);
                foreach (var filePath in multimediaFilePaths)
                {
                    await using var readStream = File.OpenRead(filePath);
                    await readStream.CopyToAsync(compressedFileStream);
                }
            }

            // Create meta.xml
            compressedFileStream.PutNextEntry("meta.xml");
            DwcArchiveMetaFileWriter.CreateMetaXmlFile(compressedFileStream, fieldDescriptions.ToList(), dwcExtensions);

            var emlFile = await _dataProviderRepository.GetEmlAsync(dataProvider.Id);
            if (emlFile == null)
            {
                _logger.LogWarning($"No eml found for provider: {dataProvider.Identifier}");
            }
            else
            {
                // Create eml.xml
                compressedFileStream.PutNextEntry("eml.xml");
                await emlFile.SaveAsync(compressedFileStream, SaveOptions.None, CancellationToken.None);
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

        private async Task WriteOccurrenceHeaderRow(ZipOutputStream compressedFileStream)
        {
            await using var streamWriter = new StreamWriter(compressedFileStream, Encoding.UTF8, -1, true);
            var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");
            _dwcArchiveOccurrenceCsvWriter.WriteHeaderRow(csvWriter,
               FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions());
            await streamWriter.FlushAsync();
        }

        private async Task WriteEmofHeaderRow(ZipOutputStream compressedFileStream)
        {
            await using var streamWriter = new StreamWriter(compressedFileStream, Encoding.UTF8, -1, true);
            var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");
            _extendedMeasurementOrFactCsvWriter.WriteHeaderRow(csvWriter);
            await streamWriter.FlushAsync();
        }

        private async Task WriteMultimediaHeaderRow(ZipOutputStream compressedFileStream)
        {
            await using var streamWriter = new StreamWriter(compressedFileStream, Encoding.UTF8, -1, true);
            var csvWriter = new NReco.Csv.CsvWriter(streamWriter, "\t");
            _simpleMultimediaCsvWriter.WriteHeaderRow(csvWriter);
            await streamWriter.FlushAsync();
        }
    }
}