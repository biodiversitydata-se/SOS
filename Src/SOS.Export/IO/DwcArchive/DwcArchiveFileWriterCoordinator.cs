using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Export.Enums;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveFileWriterCoordinator : Interfaces.IDwcArchiveFileWriterCoordinator
    {
        private readonly object _initWriteCsvLock = new object();
        private readonly IFileService _fileService;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly DwcaFilesCreationConfiguration _dwcaFilesCreationConfiguration;
        private readonly ILogger<DwcArchiveFileWriterCoordinator> _logger;
        private Dictionary<DataProvider, DwcaFilePartsInfo> _dwcaFilePartsInfoByDataProvider;

        public DwcArchiveFileWriterCoordinator(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IFileService fileService,
            DwcaFilesCreationConfiguration dwcaFilesCreationConfiguration,
            ILogger<DwcArchiveFileWriterCoordinator> logger)
        {
            _dwcArchiveFileWriter = dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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
            // todo - change name to [WriteHeaderlessDwcaFile] or [WriteHeaderlessDwcaFileParts] ?
            try
            {
                if (!_dwcaFilesCreationConfiguration.IsEnabled) return true;
                if (string.IsNullOrEmpty(dataProvider.Identifier)) return false;
                if (batchId == null) batchId = "";
                Dictionary<DwcaFilePart, string> filePathByFilePart;
                lock (_initWriteCsvLock)
                {
                    if (!_dwcaFilePartsInfoByDataProvider.TryGetValue(dataProvider, out var dwcaFilePartsInfo))
                    {
                        dwcaFilePartsInfo = CreateDwcaFilePartsInfo(dataProvider);
                    }

                    filePathByFilePart = dwcaFilePartsInfo.GetOrCreateFilePathByFilePart(batchId);
                }

                // Exclude sensitive species. Replace this implementation when the protected species implementation is finished.
                var publicObservations =  processedObservations
                    .Where(observation => !ProtectedSpeciesHelper.IsSensitiveSpecies(observation.Taxon.Id)).ToArray();
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
                var dwcaCreationTasks = new List<Task<string>>();
                foreach (var dwcaFileCreationInfo in _dwcaFilePartsInfoByDataProvider.Values)
                {
                    dwcaCreationTasks.Add(_dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                        _dwcaFilesCreationConfiguration.FolderPath, dwcaFileCreationInfo));
                }

                dwcaCreationTasks.Add(_dwcArchiveFileWriter.CreateCompleteDwcArchiveFileAsync(_dwcaFilesCreationConfiguration.FolderPath,
                    _dwcaFilePartsInfoByDataProvider.Values));
                var createdDwcaFiles = await Task.WhenAll(dwcaCreationTasks);
                
                DeleteTemporaryCreatedCsvFiles();

                return createdDwcaFiles
                    .Where(fn => !string.IsNullOrEmpty(fn))
                    .Select(fn => fn);
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
