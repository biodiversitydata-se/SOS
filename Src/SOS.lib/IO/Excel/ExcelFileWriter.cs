using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SOS.Lib.Enums;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace SOS.Lib.IO.Excel
{
    /// <summary>
    /// Excel file writer.
    /// </summary>
    public class ExcelFileWriter : FileWriterBase, IExcelFileWriter
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ILogger<ExcelFileWriter> _logger;

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExcelFileWriter(IProcessedObservationCoreRepository processedObservationRepository, 
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<ExcelFileWriter> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<FileExportResult> CreateFileAync(SearchFilter filter, 
            string exportPath,
            string fileName, 
            string culture,
            PropertyLabelType propertyLabelType,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                string excelFilePath = null;
                int nrObservations = 0;
                var propertyFields =
                    ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields);
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }
                
                var expectedNoOfObservations = await _processedObservationRepository.GetMatchCountAsync(filter);
                var stopwatch = Stopwatch.StartNew();
                _logger.LogDebug($"Excel export. Begin ES Scroll call for file: {fileName}");
                var searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
                stopwatch.Stop();
                _logger.LogDebug($"Excel export. End ES Scroll call for file: {fileName}. Elapsed: {stopwatch.ElapsedMilliseconds/1000}s");
                var fileCount = 0;
                var rowIndex = 0;
                ExcelPackage package = null;
                ExcelWorksheet sheet = null;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var packageSaveTasks = new List<Task>();

                while (searchResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    // Start fetching next batch of observations.
                    var searchResultTask = _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter);

                    // Fetch observations from ElasticSearch.
                    var processedObservations = searchResult.Records.ToArray();
                    
                    // Resolve vocabulary values.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture);
                    
                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        var flatObservation = new FlatObservation(observation);
                        // Max 100000 observations rows in a file
                        if (rowIndex % 100002 == 0)
                        {
                            packageSaveTasks.Add(TrySavePackageAync(package));

                            // Create new file
                            fileCount++;
                            excelFilePath = Path.Combine(temporaryZipExportFolderPath, $"{fileCount}-Observations.xlsx");
                            var file = new FileInfo(excelFilePath);
                            package = new ExcelPackage(file);
                            sheet = package.Workbook.Worksheets.Add("Observations");
                            WriteHeader(sheet, propertyFields, propertyLabelType);
                            rowIndex = 2;
                        }

                        int columnIndex = 1;
                        foreach (var propertyField in propertyFields)
                        {
                            var value = flatObservation.GetValue(propertyField);
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
                    // Get next batch of observations.
                    stopwatch.Restart();
                    _logger.LogDebug($"Excel export. Begin ES Scroll call for file: {fileName}");
                    searchResult = await searchResultTask;
                    stopwatch.Stop();
                    _logger.LogDebug($"Excel export. End ES Scroll call for file: {fileName}. Elapsed: {stopwatch.ElapsedMilliseconds / 1000}s");
                }

                // If less tha 99% of expected observations where fetched, something is wrong
                if (nrObservations < expectedNoOfObservations * 0.99)
                {
                    throw new Exception($"Excel export expected {expectedNoOfObservations} but only got {nrObservations}");
                }

                packageSaveTasks.Add(TrySavePackageAync(package));
                await Task.WhenAll(packageSaveTasks);

                if (gzip)
                {
                    await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                    var zipFilePath = _fileService.CompressFolder(exportPath, fileName);
                    return new FileExportResult
                    {
                        FilePath = zipFilePath,
                        NrObservations = nrObservations
                    };
                }
                else
                {
                    var destinationFilePath = Path.Combine(exportPath, $"{fileName}.xlsx");
                    File.Move(excelFilePath, destinationFilePath);
                    return new FileExportResult
                    {
                        FilePath = destinationFilePath,
                        NrObservations = nrObservations
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
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
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
                
                worksheet.Cells[firstDataRow, columnIndex, 500000+1, columnIndex].Style.Numberformat.Format = format;
                columnIndex++;
            }
        }
    }
}
