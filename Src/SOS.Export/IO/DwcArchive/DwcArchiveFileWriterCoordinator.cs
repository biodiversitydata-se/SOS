using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveFileWriterCoordinator : Interfaces.IDwcArchiveFileWriterCoordinator
    {
        public static bool IsEnabled = false;
        private readonly IFileService _fileService;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly ILogger<DwcArchiveFileWriterCoordinator> _logger;
        private Dictionary<DataProvider, DwcaFilesCreationInfo> _dwcaFilesCreationInfoByDataProvider;
        private string _exportFolderPath = @"c:\temp"; // todo - change to read from settings.

        public DwcArchiveFileWriterCoordinator(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IFileService fileService,
            ILogger<DwcArchiveFileWriterCoordinator> logger)
        {
            _dwcArchiveFileWriter = dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initialize DarwinCore CSV files creation.
        /// </summary>
        public void BeginWriteDwcCsvFiles()
        {
            _dwcaFilesCreationInfoByDataProvider = new Dictionary<DataProvider, DwcaFilesCreationInfo>();
        }

        /// <summary>
        /// Write processed observations to DarwinCore CSV file(s) for a specific data provider.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="dataProvider"></param>
        /// <param name="batchId">If the processing is done in parallel for a data provider, use the batchId to identify tha specifc batch that was processed.</param>
        /// <returns></returns>
        public async Task<bool> WriteObservations(
            IEnumerable<ProcessedObservation> processedObservations, 
            DataProvider dataProvider,
            string batchId = "")
        {
            if (!IsEnabled) return true;
            if (batchId == null) throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}() does not support the value null", nameof(batchId));
            Dictionary<DwcaFilePart, string> filePathByFilePart;
            if (!_dwcaFilesCreationInfoByDataProvider.ContainsKey(dataProvider))
            {
                _dwcaFilesCreationInfoByDataProvider.Add(dataProvider, DwcaFilesCreationInfo.Create(dataProvider, _exportFolderPath));
            }
            var dwcaFilesCreationInfo = _dwcaFilesCreationInfoByDataProvider[dataProvider];
            _fileService.CreateFolder(dwcaFilesCreationInfo.ExportFolder);
            if (dwcaFilesCreationInfo.FilePathByBatchIdAndFilePart.ContainsKey(batchId))
            {
                filePathByFilePart = dwcaFilesCreationInfo.FilePathByBatchIdAndFilePart[batchId];
            }
            else
            {
                filePathByFilePart = CreateFilePathByFilePart(dwcaFilesCreationInfo, batchId);
                dwcaFilesCreationInfo.FilePathByBatchIdAndFilePart.Add(batchId, filePathByFilePart);
            }

            var dwcObservations = processedObservations.ToDarwinCore();
            await _dwcArchiveFileWriter.WriteObservations(dwcObservations, filePathByFilePart);
            return true;
        }

        private static Dictionary<DwcaFilePart, string> CreateFilePathByFilePart(DwcaFilesCreationInfo dwcaFilesCreationInfo, string batchId)
        {
            string occurrenceCsvFilename = batchId == "" ? "Occurrence.csv" : $"Occurrence-{batchId}.csv";
            string emofCsvFilename = batchId == "" ? "Emof.csv" : $"Emof-{batchId}.csv";
            string multimediaCsvFilename = batchId == "" ? "Multimedia.csv" : $"Multimedia-{batchId}.csv";

            var filePathByFilePart = new Dictionary<DwcaFilePart, string>
            {
                {DwcaFilePart.Occurrence, Path.Combine(dwcaFilesCreationInfo.ExportFolder,  occurrenceCsvFilename)},
                {DwcaFilePart.Emof, Path.Combine(dwcaFilesCreationInfo.ExportFolder, emofCsvFilename)},
                {DwcaFilePart.Multimedia, Path.Combine(dwcaFilesCreationInfo.ExportFolder, multimediaCsvFilename)}
            };

            return filePathByFilePart;
        }

        public void DeleteCreatedCsvFiles()
        {
            
        }

        /// <summary>
        /// Create DwC-A for each data provider and DwC-A for all data providers combined.
        /// </summary>
        public async Task CreateDwcaFilesFromCreatedCsvFiles()
        {
            if (!IsEnabled) return;
            foreach (var dwcaFileCreationInfo in _dwcaFilesCreationInfoByDataProvider.Values)
            {
                await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(dwcaFileCreationInfo);
            }
        }
    }

    public class DwcaFilesCreationInfo
    {
        public DataProvider DataProvider { get; set; }
        public string ExportFolder { get; set; }
        public Dictionary<string, Dictionary<DwcaFilePart, string>> FilePathByBatchIdAndFilePart { get; set; }

        public static DwcaFilesCreationInfo Create(DataProvider dataProvider, string exportFolderPath)
        {
            var dwcaFilesCreationInfo = new DwcaFilesCreationInfo();
            dwcaFilesCreationInfo.DataProvider = dataProvider;
            dwcaFilesCreationInfo.ExportFolder = Path.Combine(exportFolderPath, $"{dataProvider.Identifier} {DateTime.Now:yyyy-MM-dd}");
            dwcaFilesCreationInfo.FilePathByBatchIdAndFilePart = new Dictionary<string, Dictionary<DwcaFilePart, string>>();
            return dwcaFilesCreationInfo;
        }
    }

    public enum DwcaFilePart
    {
        Occurrence,
        Emof,
        Multimedia
    }
}
