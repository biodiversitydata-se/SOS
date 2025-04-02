using Elastic.Clients.Elasticsearch;
using Hangfire;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.IO.Excel
{
    /// <summary>
    /// Excel file writer.
    /// </summary>
    public class ExcelFileWriter : FileWriterBase, IExcelFileWriter
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IProjectManager _projectManager;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly IGeneralizationResolver _generalizationResolver;
        private readonly ILogger<ExcelFileWriter> _logger;
        private const int FastSearchLimit = 10000;

        private void FormatColumns(ExcelWorksheet worksheet, List<PropertyFieldDescription> propertyFields)
        {
            const int firstDataRow = 2;
            //// ObservationId
            //worksheet.Cells[firstDataRow, 1, lastDataRow, 1].Style.Numberformat.Format = "0";

            int columnIndex = 1;
            // Format columns
            foreach (var fieldDescription in propertyFields)
            {
                var format = fieldDescription.DataType switch
                {
                    "DateTime" => "",  // Since Excel doesn't handle dates before 1900 we can't set date format
                    //"Double" => "#.###############",
                    "Int32" or "Int64" => "0",
                    _ => "General"
                };

                worksheet.Cells[firstDataRow, columnIndex, 500000 + 1, columnIndex].Style.Numberformat.Format = format;
                columnIndex++;
            }
        }

        /// <summary>
        /// Try to save Excel packagec# async method not 
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        private async Task TrySavePackageAync(ExcelPackage package)
        {
            if (package == null)
            {
                return;
            }

            // Save to file
            _logger.LogDebug($"Begin save Excel export. {package.File.FullName}");
            await package.SaveAsync();
            _logger.LogDebug($"Finish save Excel export. {package.File.FullName}");
            package.Dispose();
        }

        private void WriteHeader(ExcelWorksheet sheet, IEnumerable<PropertyFieldDescription> propertyFields, PropertyLabelType propertyLabelType)
        {
            if (!propertyFields?.Any() ?? true)
            {
                return;
            }
            _logger.LogDebug($"Start write Excel header");
            int columnIndex = 1;
            foreach (var propertyField in propertyFields)
            {
                string title =
                    ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(propertyField, propertyLabelType);
                sheet.Cells[1, columnIndex].Value = title;
                sheet.Cells[1, columnIndex].Style.Font.Bold = true;
                sheet.Cells[1, columnIndex].Style.Font.Color.SetColor(Color.FromArgb(255, 255, 255));
                sheet.Cells[1, columnIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, columnIndex].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                sheet.Column(columnIndex).AutoFit(10, 70);
                columnIndex++;
            }
            _logger.LogDebug($"Finish write Excel header");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="projectManager"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="generalizationResolver"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExcelFileWriter(IProcessedObservationCoreRepository processedObservationRepository,
            IProjectManager projectManager,
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            IGeneralizationResolver generalizationResolver,
            ILogger<ExcelFileWriter> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _generalizationResolver = generalizationResolver ?? throw new ArgumentNullException(nameof(generalizationResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FileExportResult> CreateFileAync(
            SearchFilter filter,
            string exportPath,
            string fileName,
            string culture,
            PropertyLabelType propertyLabelType,
            bool dynamicProjectDataFields,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {            
            var temporaryZipExportFolderPath = Path.Combine(exportPath, "zip");

            try
            {
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                var expectedNoOfObservations = (int)await _processedObservationRepository.GetMatchCountAsync(filter);
                bool useFastSearch = expectedNoOfObservations <= FastSearchLimit;
                (int nrObservations, int fileCount, List<ExcelPackage> excelPackages) result = await WriteExcelFiles(filter, fileName, culture, propertyLabelType, dynamicProjectDataFields, temporaryZipExportFolderPath, expectedNoOfObservations, useFastSearch, null, cancellationToken);

                // If more than one file created, we must return zip file
                if (gzip || result.fileCount > 1)
                {
                    await StoreFilterAsync(temporaryZipExportFolderPath, filter);

                    var zipFilePath = Path.Join(exportPath, $"{fileName}.zip");
                    _fileService.CompressDirectory(temporaryZipExportFolderPath, zipFilePath);

                    return new FileExportResult
                    {
                        FilePath = zipFilePath,
                        NrObservations = result.nrObservations
                    };
                }
                else
                {
                    var destinationFilePath = Path.Combine(exportPath, $"{fileName}.xlsx");
                    _fileService.MoveFile(result.excelPackages.First().File.FullName, destinationFilePath);
                    return new FileExportResult
                    {
                        FilePath = destinationFilePath,
                        NrObservations = result.nrObservations
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Excel File.");
                throw;
            }
            finally
            {
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }

        public async Task<(Stream stream, string filename)> CreateFileInMemoryAsync(SearchFilter filter,
           string culture,
           PropertyLabelType propertyLabelType,
           bool dynamicProjectDataFields,
           bool gzip,
           IJobCancellationToken cancellationToken)
        {
            MemoryStream memoryStream = new MemoryStream();
            MemoryStream zipMemoryStream = null;

            try
            {
                int expectedNoOfObservations = (int)await _processedObservationRepository.GetMatchCountAsync(filter);
                bool useFastSearch = expectedNoOfObservations <= FastSearchLimit;
                string fileName = $"Observations {DateTime.Now.ToString("yyyy-MM-dd HH.mm")} SOS export";                
                (int nrObservations, int fileCount, List<ExcelPackage> excelPackages) result = await WriteExcelFiles(filter, fileName, culture, propertyLabelType, dynamicProjectDataFields, null, expectedNoOfObservations, useFastSearch, memoryStream, cancellationToken);

                if (gzip)
                {
                    zipMemoryStream = new MemoryStream();
                    using (var zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        var excelFileEntry = zipArchive.CreateEntry($"{fileName}.xlsx", System.IO.Compression.CompressionLevel.NoCompression); // xlsx is already a compressed format.                       
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        using (var excelFileZipStream = excelFileEntry.Open())
                        {
                            memoryStream.CopyTo(excelFileZipStream);                            
                        }

                        var jsonEntry = zipArchive.CreateEntry("filter.json", System.IO.Compression.CompressionLevel.Fastest);
                        using (var filterFileZipStream = jsonEntry.Open())
                        using (var writer = new StreamWriter(filterFileZipStream, Encoding.UTF8))
                        {
                            writer.Write(filter.GetFilterAsJson());
                        }
                    }

                    await memoryStream.DisposeAsync();
                    memoryStream = zipMemoryStream;
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return (memoryStream, fileName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Excel file.");
                if (memoryStream != null) memoryStream.Dispose();
                if (zipMemoryStream != null) zipMemoryStream.Dispose();
                throw;
            }
        }

        private async Task<(int nrObservations, int fileCount, List<ExcelPackage> excelPackages)> WriteExcelFiles(
            SearchFilter filter,
            string fileName,
            string culture,
            PropertyLabelType propertyLabelType,
            bool dynamicProjectDataFields,
            string temporaryZipExportFolderPath,
            int expectedNoOfObservations,
            bool useFastSearch,
            Stream stream,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                int nrObservations = 0;
                var projectIds = await _processedObservationRepository.GetProjectIdsAsync(filter);
                var projects = await _projectManager.GetAsync(projectIds);
                _logger.LogInformation($"Exporting projects to Excel. ProjectIds.Count={projectIds?.Count()}, Projects.Count={projects?.Count()}, dynamicProjectDataFields={dynamicProjectDataFields}");

                var excelPackages = new List<ExcelPackage>();
                var propertyFields = dynamicProjectDataFields && (projects?.Any() ?? false) ?
                    ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields, projects: projects)
                    :
                    ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields);
                Models.Search.Result.PagedResult<dynamic> fastSearchResult = null;
                Models.Search.Result.SearchAfterResult<Observation, IReadOnlyCollection<FieldValue>> searchResult = null;
                if (useFastSearch)
                {
                    fastSearchResult = await _processedObservationRepository.GetChunkAsync<dynamic>(filter, 0, 10000);
                }
                else
                {
                    searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
                }

                var fileCount = 0;
                var rowIndex = 0;
                ExcelPackage package = null;
                ExcelWorksheet sheet = null;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var packageSaveTasks = new List<Task>();
                var excelStreams = new List<Stream>();

                while ((useFastSearch && (fastSearchResult?.Records?.Any() ?? false)) || (!useFastSearch && (searchResult?.Records?.Any() ?? false)))
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    Observation[] processedObservations = null;
                    if (useFastSearch)
                    {
                        processedObservations = fastSearchResult.Records.ToObservationsArray();                        
                    }
                    else
                    {
                        processedObservations = searchResult.Records.ToArray();                        
                    }

                    // Resolve vocabulary values.
                    var debugObs = processedObservations.FirstOrDefault(m => m.Occurrence.OccurrenceId == "urn:lsid:artportalen.se:sighting:75884919");
                    if (debugObs != null)
                    {
                        _logger.LogInformation($"Values before generalization resolve: CoordinateUncertaintyInMeters={debugObs.Location.CoordinateUncertaintyInMeters}, DecimalLongitude={debugObs.Location.DecimalLongitude}, DecimalLatitude={debugObs.Location.DecimalLatitude}");
                    }
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture);
                    await _generalizationResolver.ResolveGeneralizedObservationsAsync(filter, processedObservations);
                    debugObs = processedObservations.FirstOrDefault(m => m.Occurrence.OccurrenceId == "urn:lsid:artportalen.se:sighting:75884919");
                    if (debugObs != null)
                    {
                        _logger.LogInformation($"Values after generalization resolve: CoordinateUncertaintyInMeters={debugObs.Location.CoordinateUncertaintyInMeters}, DecimalLongitude={debugObs.Location.DecimalLongitude}, DecimalLatitude={debugObs.Location.DecimalLatitude}");
                    }

                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        var flatObservation = new FlatObservation(observation);
                        // Max 100000 observations rows in a file
                        if (rowIndex % 100002 == 0)
                        {                            
                            // Create new file
                            fileCount++;
                            if (stream != null)
                            {
                                package = new ExcelPackage(stream); // save to stream instead of file.
                            }
                            else
                            {
                                packageSaveTasks.Add(TrySavePackageAync(package));
                                var excelFilePath = Path.Combine(temporaryZipExportFolderPath, $"{fileName}{(fileCount > 1 ? $" ({fileCount})" : string.Empty)}.xlsx");
                                var file = new FileInfo(excelFilePath);
                                package = new ExcelPackage(file);
                                excelPackages.Add(package);                                
                            }

                            sheet = package.Workbook.Worksheets.Add("Observations");
                            WriteHeader(sheet, propertyFields, propertyLabelType);
                            rowIndex = 2;
                        }

                        int columnIndex = 1;
                        foreach (var propertyField in propertyFields)
                        {
                            var value = dynamicProjectDataFields && propertyField.IsDynamicCreated ? flatObservation.GetDynamicValue(propertyField) : flatObservation.GetValue(propertyField);
                            object val = value == null ? null : propertyField.DataTypeEnum switch
                            {
                                PropertyFieldDataType.Boolean => ((bool?)value)?.ToString(CultureInfo.InvariantCulture),
                                PropertyFieldDataType.DateTime => ((DateTime?)value)?.ToShortDateString(),
                                _ => value
                            };

                            sheet.Cells[rowIndex, columnIndex].Value = val;
                            columnIndex++;
                        }

                        rowIndex++;
                    }
                    
                    nrObservations += processedObservations.Length;
                    processedObservations = null;
                    if (useFastSearch) break;
                    searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter == null ? null : [searchResult.SearchAfter.ToFieldValue()]);
                }

                fastSearchResult = null;
                searchResult = null;
                // If less tha 99% of expected observations where fetched, something is wrong
                if (nrObservations < expectedNoOfObservations * 0.99)
                {
                    throw new Exception($"Excel export expected {expectedNoOfObservations} but only got {nrObservations}");
                }

                if (stream != null)
                {
                    await package.SaveAsync();                    
                }
                else
                {
                    packageSaveTasks.Add(TrySavePackageAync(package));
                    await Task.WhenAll(packageSaveTasks);
                }
                
                return (nrObservations, fileCount, excelPackages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating Excel file");
                throw;
            }
        }
    }
}
