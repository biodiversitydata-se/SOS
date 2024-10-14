﻿using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Factories;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.IO.DwcArchive
{
    public class DwcArchiveFileWriterCoordinator : IDwcArchiveFileWriterCoordinator
    {
        private readonly IFileService _fileService;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly IDwcArchiveEventFileWriter _dwcArchiveEventFileWriter;
        private readonly DwcaFilesCreationConfiguration _dwcaFilesCreationConfiguration;
        private readonly ILogger<DwcArchiveFileWriterCoordinator> _logger;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly IVerbatimClient _importClient;

        private Dictionary<DataProvider, DwcaFilePartsInfo> _dwcaFilePartsInfoByDataProvider;
        private WrittenEventSets _writtenEventSets;
        public bool Enabled => _dwcaFilesCreationConfiguration.IsEnabled;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Simplified by only returning file size
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>l
        private async Task<string> GetFileHashAsync(string path)
        {
            try
            {
                using var zip = ZipFile.OpenRead(path);
                var fileSize = 0L;
                foreach (var zipEntry in zip.Entries)
                {
                    if (zipEntry.Name.Equals("eml.xml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        fileSize += await GetEmlFileSizeWithoutPubDateAsync(zipEntry);
                    }
                    else
                    {
                        fileSize += zipEntry.Length;
                    }
                }

                return fileSize.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<long> GetEmlFileSizeWithoutPubDateAsync(ZipArchiveEntry emlZipEntry)
        {
            try
            {
                await using var stream = emlZipEntry.Open();
                var size = await DwCArchiveEmlFileFactory.GetEmlSizeWithoutPubDateAsync(stream);
                return size;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public DwcArchiveFileWriterCoordinator(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IDwcArchiveEventFileWriter dwcArchiveEventFileWriter,
            IFileService fileService,
            IDataProviderRepository dataProviderRepository,
            IVerbatimClient importClient,
            DwcaFilesCreationConfiguration dwcaFilesCreationConfiguration,
            ILogger<DwcArchiveFileWriterCoordinator> logger)
        {
            _dwcArchiveFileWriter = dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _dwcArchiveEventFileWriter = dwcArchiveEventFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveEventFileWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _dataProviderRepository = dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _importClient = importClient ??
                            throw new ArgumentNullException(
                                nameof(importClient));
            _dwcaFilesCreationConfiguration = dwcaFilesCreationConfiguration ?? throw new ArgumentNullException(nameof(dwcaFilesCreationConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initialize DarwinCore CSV files creation.
        /// </summary>
        public void BeginWriteDwcCsvFiles()
        {
            _dwcaFilePartsInfoByDataProvider = new Dictionary<DataProvider, DwcaFilePartsInfo>();
            _writtenEventSets = new WrittenEventSets();
        }

        /// <summary>
        /// Write processed observations to DarwinCore CSV file(s) for a specific data provider.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="dataProvider"></param>
        /// <param name="batchId">If the processing is done in parallel for a data provider, use the batchId to identify the specific batch that was processed.</param>
        /// <returns></returns>
        public async Task<bool> WriteHeaderlessDwcaFileParts(
            IEnumerable<Observation> processedObservations,
            DataProvider dataProvider,
            string batchId = "")
        {
            if (!Enabled || !(processedObservations?.Any() ?? false))
            {
                return true;
            }

            try
            {
                if (string.IsNullOrEmpty(dataProvider?.Identifier))
                {
                    return false;
                }

                batchId ??= "";
                Dictionary<DwcaFilePart, string> filePathByFilePart;
                Dictionary<DwcaEventFilePart, string> filePathByEventFilePart;
                DwcaFilePartsInfo dwcaFilePartsInfo = null;

                await _semaphore.WaitAsync();
                try
                {
                    if (!_dwcaFilePartsInfoByDataProvider?.TryGetValue(dataProvider, out dwcaFilePartsInfo) ?? true)
                    {
                        dwcaFilePartsInfo = CreateDwcaFilePartsInfo(dataProvider);
                    }

                    filePathByFilePart = dwcaFilePartsInfo.GetOrCreateFilePathByFilePart(batchId);
                    filePathByEventFilePart = dwcaFilePartsInfo.GetOrCreateEventFilePathByFilePart(batchId);
                }
                finally
                {
                    _semaphore.Release();
                }

                // Exclude sensitive species.
                var publicObservations = processedObservations
                    .Where(observation => !(observation.AccessRights != null && (AccessRightsId)observation.AccessRights.Id == AccessRightsId.NotForPublicUsage)).ToArray();
                var writeHeaderlessDwcaFilesTasks = new List<Task<DwcaWriteResult>>();
                Task<DwcaWriteResult> writeOccurrenceBatchTask = _dwcArchiveFileWriter.WriteHeaderlessDwcaFiles(dataProvider, publicObservations, filePathByFilePart, dwcaFilePartsInfo, _dwcaFilesCreationConfiguration.CheckForIllegalCharacters);
                writeHeaderlessDwcaFilesTasks.Add(writeOccurrenceBatchTask);
                Task<DwcaWriteResult> writeEventBatchTask = null;
                if (dataProvider.CreateEventDwC)
                {
                    writeEventBatchTask = _dwcArchiveEventFileWriter.WriteHeaderlessEventDwcaFilesAsync(dataProvider, publicObservations, filePathByEventFilePart, _writtenEventSets, _dwcaFilesCreationConfiguration.CheckForIllegalCharacters);
                    writeHeaderlessDwcaFilesTasks.Add(writeEventBatchTask);
                }

                DwcaWriteResult[] results = await Task.WhenAll(writeHeaderlessDwcaFilesTasks);

                // Update write summary
                await _semaphore.WaitAsync();
                try
                {
                    if (writeOccurrenceBatchTask != null)
                    {
                        UpdateWriteSummary(dwcaFilePartsInfo.OccurrenceDwcaWriteSummary, writeOccurrenceBatchTask.Result);
                    }

                    if (writeEventBatchTask != null)
                    {
                        UpdateWriteSummary(dwcaFilePartsInfo.EventDwcaWriteSummary, writeEventBatchTask.Result);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Write observations failed for {dataProvider} and batchId={batchId}");
                return false;
            }
        }

        private void UpdateWriteSummary(DwcaWriteResult summary, DwcaWriteResult batch)
        {
            summary.OccurrenceCount += batch.OccurrenceCount;
            summary.EventCount += batch.EventCount;
            summary.MultimediaCount += batch.MultimediaCount;
            summary.EmofCount += batch.EmofCount;
        }

        /// <summary>
        /// Create DwC-A for each data provider and DwC-A for all data providers combined.
        /// </summary>
        /// <returns>A list with file paths to all created DwC-A files.</returns>
        public async Task<IEnumerable<string>> CreateDwcaFilesFromCreatedCsvFiles()
        {
            try
            {
                if (!Enabled) return null;
                var dwcaCreationTasks = new Dictionary<(DataProvider dataProvider, bool eventBased), Task<string>>();
                foreach (var pair in _dwcaFilePartsInfoByDataProvider)
                {
                    var provider = pair.Key;
                    if (provider.UseVerbatimFileInExport)
                    {
                        try
                        {
                            var darwinCoreArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(provider, _importClient, _logger);
                            // try to get source file
                            var sourceStream = await darwinCoreArchiveVerbatimRepository.GetSourceFileAsync(provider.Id);

                            if (sourceStream != null)
                            {
                                // Store source file on disk and add it to file tasks
                                var filePath = Path.Combine(_dwcaFilesCreationConfiguration.FolderPath, $"{provider.Identifier}.dwca.zip");

                                if (File.Exists(filePath))
                                {
                                    File.Delete(filePath);
                                }

                                var fileStream = File.OpenWrite(filePath);

                                sourceStream.Seek(0, SeekOrigin.Begin);
                                await sourceStream.CopyToAsync(fileStream);
                                await fileStream.FlushAsync();
                                await fileStream.DisposeAsync();
                                await sourceStream.DisposeAsync();

                                dwcaCreationTasks.Add((provider, false), Task.FromResult(filePath));
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to use source file in export");
                        }

                        continue;
                    }

                    dwcaCreationTasks.Add((provider, false), _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(provider,
                        _dwcaFilesCreationConfiguration.FolderPath, pair.Value));
                    if (provider.CreateEventDwC)
                    {
                        dwcaCreationTasks.Add((provider, true), _dwcArchiveEventFileWriter.CreateEventDwcArchiveFileAsync(provider,
                        _dwcaFilesCreationConfiguration.FolderPath, pair.Value));
                    }
                }

                dwcaCreationTasks.Add((new DataProvider { Id = 0, Identifier = "SOS Complete" }, false), _dwcArchiveFileWriter.CreateCompleteDwcArchiveFileAsync(_dwcaFilesCreationConfiguration.FolderPath,
                    _dwcaFilePartsInfoByDataProvider.Values));
                await Task.WhenAll(dwcaCreationTasks.Values);

                var createdDwcaFiles = new List<string>();
                var completeDwcArchiveFilePath = string.Empty;

                foreach (var task in dwcaCreationTasks)
                {
                    var dataProvider = task.Key.dataProvider;
                    var filePath = task.Value.Result;

                    // Id 0 = complete Dwc archive file
                    if (dataProvider.Id == 0)
                    {
                        completeDwcArchiveFilePath = filePath;
                        continue;
                    }

                    var filePartsInfo = _dwcaFilePartsInfoByDataProvider[task.Key.dataProvider];
                    var hash = await GetFileHashAsync(task.Value.Result);
                    if (task.Key.eventBased)
                    {
                        var writeSummary = filePartsInfo.EventDwcaWriteSummary;
                        _logger.LogInformation($"Generated event based DwC-A file for {dataProvider}. EventCount={writeSummary.EventCount:N0} OccurrenceCount={writeSummary.OccurrenceCount:N0}, EmofCount={writeSummary.EmofCount:N0}, MultimediaCount={writeSummary.MultimediaCount:N0}, Old Hash=\"{dataProvider.LatestUploadedFileHash}\", New Hash=\"{hash}\"");
                    }
                    else
                    {
                        var writeSummary = filePartsInfo.OccurrenceDwcaWriteSummary;
                        _logger.LogInformation($"Generated DwC-A file for {dataProvider}. OccurrenceCount={writeSummary.OccurrenceCount:N0}, EmofCount={writeSummary.EmofCount:N0}, MultimediaCount={writeSummary.MultimediaCount:N0}, Old Hash=\"{dataProvider.LatestUploadedFileHash}\", New Hash=\"{hash}\"");
                    }

                    if (dataProvider.LatestUploadedFileHash == hash)
                    {
                        continue;
                    }

                    createdDwcaFiles.Add(task.Value.Result);
                    dataProvider.LatestUploadedFileHash = hash;
                    await _dataProviderRepository.UpdateAsync(dataProvider.Id, dataProvider);
                }

                // If any file is changed, add complete file
                if (!string.IsNullOrEmpty(completeDwcArchiveFilePath) && createdDwcaFiles.Any())
                {
                    createdDwcaFiles.Add(completeDwcArchiveFilePath);
                }
                else
                {
                    File.Delete(completeDwcArchiveFilePath);
                }

                DeleteTemporaryCreatedCsvFiles();

                return createdDwcaFiles;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Merge CSV files into DwC-A failed");
                return null;
            }
        }

        private DwcaFilePartsInfo CreateDwcaFilePartsInfo(DataProvider dataProvider)
        {
            var dwcaFilePartsInfo = DwcaFilePartsInfo.Create(dataProvider, _dwcaFilesCreationConfiguration.FolderPath);
            _dwcaFilePartsInfoByDataProvider.Add(dataProvider, dwcaFilePartsInfo);

            if (!Directory.Exists(dwcaFilePartsInfo.ExportFolder))
            {
                _fileService.CreateDirectory(dwcaFilePartsInfo.ExportFolder);
            }
            else
            {
                // Empty folder from txt files before we start to create new ones.
                foreach (string file in Directory.GetFiles(dwcaFilePartsInfo.ExportFolder, "*.txt")
                    .Where(item => item.EndsWith(".txt")))
                {
                    File.Delete(file);
                }
            }

            return dwcaFilePartsInfo;
        }

        /// <summary>
        /// Delete temporary created CSV files.
        /// </summary>
        public void DeleteTemporaryCreatedCsvFiles()
        {
            if (_dwcaFilePartsInfoByDataProvider == null) return;
            foreach (var dwcaFileCreationInfo in _dwcaFilePartsInfoByDataProvider.Values)
            {
                foreach (var filePath in dwcaFileCreationInfo.FilePathByBatchIdAndFilePart.Values.SelectMany(f => f.Values))
                {
                    try
                    {
                        if (File.Exists(filePath)) File.Delete(filePath);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to delete file: {filePath}");
                    }
                }

                foreach (var filePath in dwcaFileCreationInfo.EventFilePathByBatchIdAndFilePart.Values.SelectMany(f => f.Values))
                {
                    try
                    {
                        if (File.Exists(filePath)) File.Delete(filePath);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to delete file: {filePath}");
                    }
                }

                try
                {
                    if (Directory.Exists(dwcaFileCreationInfo.ExportFolder)) Directory.Delete(dwcaFileCreationInfo.ExportFolder);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to delete directory: {dwcaFileCreationInfo.ExportFolder}");
                }
            }
        }
    }
}
