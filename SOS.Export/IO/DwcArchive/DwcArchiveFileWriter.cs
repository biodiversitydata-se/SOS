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

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveFileWriter : IDwcArchiveFileWriter
    {
        private readonly IDwcArchiveOccurrenceCsvWriter _dwcArchiveOccurrenceCsvWriter;
        private readonly IFileService _fileService;
        private readonly ILogger<DwcArchiveFileWriter> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dwcArchiveOccurrenceCsvWriter"></param>
        /// <param name="fileService"></param>
        /// <param name="logger"></param>
        public DwcArchiveFileWriter(
            IDwcArchiveOccurrenceCsvWriter dwcArchiveOccurrenceCsvWriter,
            IFileService fileService,
            ILogger<DwcArchiveFileWriter> logger)
        {
            _dwcArchiveOccurrenceCsvWriter = dwcArchiveOccurrenceCsvWriter ?? throw new ArgumentNullException(nameof(dwcArchiveOccurrenceCsvWriter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> CreateDwcArchiveFileAsync(
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            string exportFolderPath,
            IJobCancellationToken cancellationToken)
        {
            return await CreateDwcArchiveFileAsync(
                processedDarwinCoreRepository,
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                exportFolderPath,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> CreateDwcArchiveFileAsync(
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            IEnumerable<FieldDescription> fieldDescriptions,
            string exportFolderPath,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;
            
            try
            {
                var zipFolderName = Guid.NewGuid().ToString();
                temporaryZipExportFolderPath = Path.Combine(exportFolderPath, zipFolderName);
                _fileService.CreateFolder(temporaryZipExportFolderPath);
                string occurrenceCsvFilePath = Path.Combine(temporaryZipExportFolderPath, "occurrence.csv");
                string metaXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "meta.xml");
                string emlXmlFilePath = Path.Combine(temporaryZipExportFolderPath, "eml.xml");

                // Create Occurrence.csv
                using (FileStream fileStream = File.Create(occurrenceCsvFilePath))
                {
                    await _dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
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

                var zipFilePath = _fileService.CompressFolder(exportFolderPath, zipFolderName);
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
                if (Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.Delete(temporaryZipExportFolderPath);
                }
            }
        }
    }
}
