using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.IO.Excel
{
    public class ExcelFileWriter : FileWriterBase, IExcelFileWriter
    {
        private readonly IProcessedPublicObservationRepository _processedPublicObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ILogger<ExcelFileWriter> _logger;

        /// <summary>
        /// Write sheet header
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="propertyIndexes"></param>
        private void WriteHeader(ExcelWorksheet sheet, Dictionary<string, int> propertyIndexes)
        {
            if (!propertyIndexes?.Any() ?? true)
            {
                return;
            }

            foreach (var propertyIndex in propertyIndexes)
            {
                sheet.Cells[1, propertyIndex.Value].Value = propertyIndex.Key;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="logger"></param>
        public ExcelFileWriter(IProcessedPublicObservationRepository processedPublicObservationRepository, 
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<ExcelFileWriter> logger)
        {
            _processedPublicObservationRepository = processedPublicObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedPublicObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> CreateFileAync(SearchFilter filter, string exportPath, string fileName,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }
                
                var scrollResult = await _processedPublicObservationRepository.ScrollObservationsAsync(filter, null);

                var objectFlattenerHelper = new ObjectFlattenerHelper();
                var propertyIndexes = new Dictionary<string, int>();
                var fileCount = 0;
                var rowIndex = 0;
                ExcelPackage package = null;
                ExcelWorksheet sheet = null;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var processedObservations = scrollResult.Records.ToArray();

                    // Convert observations to DwC format.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);

                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        // Max 500000 rows in a file
                        if (rowIndex % 500000 == 0)
                        {
                            // If we have a package, save it
                            if (package != null)
                            {
                                WriteHeader(sheet, propertyIndexes);

                                // Save to file
                                await package.SaveAsync();
                                sheet.Dispose();
                                package.Dispose();
                            }

                            // Create new file
                            fileCount++;
                            var file = new FileInfo(Path.Combine(temporaryZipExportFolderPath, $"{fileCount}-{fileName}.xlsx"));
                            package = new ExcelPackage(file);
                            sheet = package.Workbook.Worksheets.Add("Observations");
                            rowIndex = 1;
                        }

                        var objectProperties = objectFlattenerHelper.Execute(observation);
                        if (objectProperties?.Any() ?? false)
                        {
                            foreach (var objectProperty in objectProperties.OrderBy(p => p.Key))
                            {
                                // Check if property is included (OutputFields empty = all)
                                if (!((!filter.OutputFields?.Any() ?? true) || filter.OutputFields.Contains(objectProperty.Key, StringComparer.CurrentCultureIgnoreCase)))
                                {
                                    continue;
                                }

                                if (!propertyIndexes.TryGetValue(objectProperty.Key, out var index))
                                {
                                    index = propertyIndexes.Count+1;
                                    propertyIndexes.Add(objectProperty.Key, index);
                                }

                                sheet.Cells[rowIndex+1, index].Value = objectProperty.Value;
                            }
                        }

                        rowIndex++;
                    }

                    // Get next batch of observations.
                    scrollResult = await _processedPublicObservationRepository.ScrollObservationsAsync(filter, scrollResult.ScrollId);
                }
                
                // If we have a package, save it
                if (package != null)
                {
                    WriteHeader(sheet, propertyIndexes);

                    // Save to file
                    await package.SaveAsync();
                    sheet.Dispose();
                    package.Dispose();
                }

                await StoreFilterAsync(temporaryZipExportFolderPath, filter);

                var zipFilePath = _fileService.CompressFolder(exportPath, fileName);

                return zipFilePath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create GeoJson File.");
                throw;
            }
            finally
            {
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
        }
    }
}
