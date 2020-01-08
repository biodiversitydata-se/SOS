using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Helpers;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveFileWriter : IDwcArchiveFileWriter
    {
        private readonly IDwcArchiveOccurrenceCsvWriter _dwcArchiveOccurrenceCsvWriter;
        private readonly IExtendedMeasurementOrFactCsvWriter _extendedMeasurementOrFactCsvWriter;
        private readonly IFileService _fileService;
        private readonly ILogger<DwcArchiveFileWriter> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dwcArchiveOccurrenceCsvWriter"></param>
        /// <param name="extendedMeasurementOrFactCsvWriter"></param>
        /// <param name="fileService"></param>
        /// <param name="logger"></param>
        public DwcArchiveFileWriter(
            IDwcArchiveOccurrenceCsvWriter dwcArchiveOccurrenceCsvWriter,
            IExtendedMeasurementOrFactCsvWriter extendedMeasurementOrFactCsvWriter,
            IFileService fileService,
            ILogger<DwcArchiveFileWriter> logger)
        {
            _dwcArchiveOccurrenceCsvWriter = dwcArchiveOccurrenceCsvWriter ?? throw new ArgumentNullException(nameof(dwcArchiveOccurrenceCsvWriter));
            _extendedMeasurementOrFactCsvWriter = extendedMeasurementOrFactCsvWriter ?? throw new ArgumentNullException(nameof(extendedMeasurementOrFactCsvWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> CreateDwcArchiveFileAsync(
            AdvancedFilter filter,
            string fileName,
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken)
        {
            return await CreateDwcArchiveFileAsync(
                filter,
                fileName,
                processedDarwinCoreRepository,
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                processInfo,
                exportFolderPath,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> CreateDwcArchiveFileAsync(
            AdvancedFilter filter,
            string fileName,
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
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
                string occurrenceCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "occurrence.csv");
                string emofCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "extendedMeasurementOrFact.csv");
                string metaXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "meta.xml");
                string emlXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "eml.xml");
                string processInfoXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "processinfo.xml");

                // Create Occurrence.csv
                using (FileStream fileStream = File.Create(occurrenceCsvFilePath))
                {
                    await _dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                        filter,
                        fileStream,
                        fieldDescriptions,
                        processedDarwinCoreRepository,
                        cancellationToken);
                }

                // Create ExtendedMeasurementOrFact.csv
                using (FileStream fileStream = File.Create(emofCsvFilePath))
                {
                    await _extendedMeasurementOrFactCsvWriter.CreateCsvFileAsync(
                        fileStream,
                        fieldDescriptions,
                        processedDarwinCoreRepository,
                        cancellationToken);
                }

                // Create meta.xml
                using (FileStream fileStream = File.Create(metaXmlFilePath))
                {
                    DwcArchiveMetaFileWriter.CreateMetaXmlFile(fileStream, fieldDescriptions.ToList());
                }

                // Create eml.xml
                using (FileStream fileStream = File.Create(emlXmlFilePath))
                {
                    await DwCArchiveEmlFileFactory.CreateEmlXmlFileAsync(fileStream);
                }

                // Create processinfo.xml
                
                await using var processInfoFileStream = File.Create(processInfoXmlFilePath);
                DwcProcessInfoFileWriter.CreateProcessInfoFile(processInfoFileStream, processInfo);
                processInfoFileStream.Close();
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
    }
}
