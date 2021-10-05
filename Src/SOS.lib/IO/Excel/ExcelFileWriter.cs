using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.IO.Excel
{
    /// <summary>
    /// Excel file writer.
    /// </summary>
    public class ExcelFileWriter : FileWriterBase, IExcelFileWriter
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ILogger<ExcelFileWriter> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="logger"></param>
        public ExcelFileWriter(IProcessedObservationRepository processedObservationRepository, 
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
        
        public async Task<string> CreateFileAync(SearchFilter filter, string exportPath,
            string fileName, string culture, OutputFieldSet outputFieldSet, PropertyLabelType propertyLabelType,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                var propertyFields = ObservationPropertyFieldDescriptionHelper.FieldsByFieldSet[outputFieldSet];
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }

                var scrollResult = await _processedObservationRepository.ScrollObservationsAsync(filter, null);
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

                    // Resolve vocabulary values.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture, true);

                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        var flatObservation = new FlatObservation(observation);
                        // Max 500000 rows in a file
                        if (rowIndex % 500000 == 0)
                        {
                            // If we have a package, save it
                            if (package != null)
                            {
                                WriteHeader(sheet, propertyFields, propertyLabelType);

                                // Save to file
                                await package.SaveAsync();
                                sheet.Dispose();
                                package.Dispose();
                            }

                            // Create new file
                            fileCount++;
                            var file = new FileInfo(Path.Combine(temporaryZipExportFolderPath, $"{fileCount}-Observations.xlsx"));
                            package = new ExcelPackage(file);
                            sheet = package.Workbook.Worksheets.Add("Observations");
                            rowIndex = 1;
                        }

                        int columnIndex = 1;
                        foreach (var propertyField in propertyFields)
                        {
                            var value = flatObservation.GetValue(propertyField);
                            if (propertyField.DataTypeEnum == PropertyFieldDataType.DateTime)
                            {
                                value = ((DateTime?)value)?.ToString("yyyy-MM-dd");
                            }
                            else if (propertyField.DataTypeEnum == PropertyFieldDataType.Boolean)
                            {
                                value = ((bool?) value)?.ToString(CultureInfo.InvariantCulture);
                            }

                            sheet.Cells[rowIndex + 1, columnIndex].Value = value;
                            columnIndex++;
                        }

                        rowIndex++;
                    }

                    // Get next batch of observations.
                    scrollResult = await _processedObservationRepository.ScrollObservationsAsync(filter, scrollResult.ScrollId);
                }

                // If we have a package, save it
                if (package != null)
                {
                    WriteHeader(sheet, propertyFields, propertyLabelType);

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
                _logger.LogError(e, "Failed to create Excel File.");
                throw;
            }
            finally
            {
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
        }

        private void WriteHeader(ExcelWorksheet sheet, List<PropertyFieldDescription> propertyFields, PropertyLabelType propertyLabelType)
        {
            if (!propertyFields?.Any() ?? true)
            {
                return;
            }

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
                string format;
                switch (fieldDescription.DataType)
                {
                    // Text format: "@"
                    // General format: "General"
                    // Date format 1: "yyyy-mm-dd"
                    // Date format 2: "yyyy-MM-dd"

                    case "DateTime":
                        // Since Excel doesn't handle dates before 1900 we can't set date format
                        format = "";
                        break;
                    case "Double":
                        format = "General";
                        //format = "#.###############";

                        break;
                    case "Int32":
                    case "Int64":
                        format = "0";
                        break;
                    default:
                        format = "General";
                        break;
                }

                worksheet.Cells[firstDataRow, columnIndex, 500000+1, columnIndex].Style.Numberformat.Format = format;
                columnIndex++;
            }
        }
    }
}
