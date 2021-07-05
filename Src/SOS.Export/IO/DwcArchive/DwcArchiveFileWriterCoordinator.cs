﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ionic.Zip;
using Microsoft.Extensions.Logging;
using SOS.Export.Enums;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveFileWriterCoordinator : Interfaces.IDwcArchiveFileWriterCoordinator
    {
        private readonly object _initWriteCsvLock = new object();
        private readonly IFileService _fileService;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly DwcaFilesCreationConfiguration _dwcaFilesCreationConfiguration;
        private readonly ILogger<DwcArchiveFileWriterCoordinator> _logger;
        private readonly IDataProviderRepository _dataProviderRepository;
        private Dictionary<DataProvider, DwcaFilePartsInfo> _dwcaFilePartsInfoByDataProvider;

        /// <summary>
        /// Simplified by only returning file size
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<string> GetFileHashAsync(string path)
        {
            try
            {
                using var zip = ZipFile.Read(path);
                long fileSize = 0;
                foreach (var zipEntry in zip.Where(m => m.FileName != "eml.xml"))
                {
                    fileSize += zipEntry.UncompressedSize;
                }
                
                var emlFile = zip.FirstOrDefault(zipEntry => zipEntry.FileName == "eml.xml");

                if (emlFile == null)
                {
                    fileSize -= 1;
                }
                else
                {
                    fileSize += await GetEmlFileSizeWithoutPubDateAsync(emlFile);
                }

                return fileSize.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<long> GetEmlFileSizeWithoutPubDateAsync(ZipEntry emlZipEntry)
        {
            try
            {
                await using var stream = new MemoryStream();
                emlZipEntry.Extract(stream);
                stream.Position = 0;
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
            IFileService fileService,
            IDataProviderRepository dataProviderRepository,
            DwcaFilesCreationConfiguration dwcaFilesCreationConfiguration,
            ILogger<DwcArchiveFileWriterCoordinator> logger)
        {
            _dwcArchiveFileWriter = dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _dataProviderRepository = dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _dwcaFilesCreationConfiguration = dwcaFilesCreationConfiguration ?? throw new ArgumentNullException(nameof(dwcaFilesCreationConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initialize DarwinCore CSV files creation.
        /// </summary>
        public void BeginWriteDwcCsvFiles()
        {
            _dwcaFilePartsInfoByDataProvider = new Dictionary<DataProvider, DwcaFilePartsInfo>();
        }

        /// <summary>
        /// Write processed observations to DarwinCore CSV file(s) for a specific data provider.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="dataProvider"></param>
        /// <param name="batchId">If the processing is done in parallel for a data provider, use the batchId to identify the specific batch that was processed.</param>
        /// <returns></returns>
        public async Task<bool> WriteObservations(
            IEnumerable<Observation> processedObservations, 
            DataProvider dataProvider,
            string batchId = "")
        {
            if (!_dwcaFilesCreationConfiguration.IsEnabled || !(processedObservations?.Any() ?? false))
            {
                return true;
            }

            // todo - change name to [WriteHeaderlessDwcaFile] or [WriteHeaderlessDwcaFileParts] ?
            try
            {
                if (string.IsNullOrEmpty(dataProvider?.Identifier)) return false;
                if (batchId == null) batchId = "";
                Dictionary<DwcaFilePart, string> filePathByFilePart;
                lock (_initWriteCsvLock)
                {
                    DwcaFilePartsInfo dwcaFilePartsInfo = null;
                    if (!_dwcaFilePartsInfoByDataProvider?.TryGetValue(dataProvider, out dwcaFilePartsInfo) ?? true)
                    {
                        dwcaFilePartsInfo = CreateDwcaFilePartsInfo(dataProvider);
                    }

                    filePathByFilePart = dwcaFilePartsInfo.GetOrCreateFilePathByFilePart(batchId);
                }

                // Exclude sensitive species. Replace this implementation when the protected species implementation is finished.
                var publicObservations =  processedObservations
                    .Where(observation => observation?.Taxon != null && !ProtectedSpeciesHelper.IsSensitiveSpecies(observation.Taxon.Id)).ToArray();
                await _dwcArchiveFileWriter.WriteHeaderlessDwcaFiles(publicObservations, filePathByFilePart);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Write observations failed for {dataProvider} and batchId={batchId}");
                return false;
            }
        }

        /// <summary>
        /// Create DwC-A for each data provider and DwC-A for all data providers combined.
        /// </summary>
        /// <returns>A list with file paths to all created DwC-A files.</returns>
        public async Task<IEnumerable<string>> CreateDwcaFilesFromCreatedCsvFiles()
        {
            try
            {
                if (!_dwcaFilesCreationConfiguration.IsEnabled) return null;
                var dwcaCreationTasks = new Dictionary<DataProvider, Task<string>>();
                foreach (var pair in _dwcaFilePartsInfoByDataProvider)
                {
                    dwcaCreationTasks.Add(pair.Key, _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(pair.Key,
                        _dwcaFilesCreationConfiguration.FolderPath, pair.Value));
                }

                dwcaCreationTasks.Add(new DataProvider(),  _dwcArchiveFileWriter.CreateCompleteDwcArchiveFileAsync(_dwcaFilesCreationConfiguration.FolderPath,
                    _dwcaFilePartsInfoByDataProvider.Values));
                await Task.WhenAll(dwcaCreationTasks.Values);

                var createdDwcaFiles = new List<string>();
                var completeDwcArchiveFilePath = string.Empty;

                foreach (var task in dwcaCreationTasks)
                {
                    var dataProvider = task.Key;
                    var filePath = task.Value.Result;
                    
                    // Id 0 = complete Dwc archive file
                    if (dataProvider.Id == 0)
                    {
                        completeDwcArchiveFilePath = filePath;
                        continue;
                    }

                    var hash = await GetFileHashAsync(task.Value.Result);

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
                _fileService.CreateFolder(dwcaFilePartsInfo.ExportFolder);
            }
            else
            {
                // Empty folder from CSV files before we start to create new ones.
                foreach (string file in Directory.GetFiles(dwcaFilePartsInfo.ExportFolder, "*.csv")
                    .Where(item => item.EndsWith(".csv")))
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
                foreach (string filePath in dwcaFileCreationInfo.FilePathByBatchIdAndFilePart.Values.SelectMany(f => f.Values))
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
