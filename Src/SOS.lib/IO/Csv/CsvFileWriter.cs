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
using SOS.Lib.Extensions;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.IO.Excel
{
    /// <summary>
    /// Excel file writer.
    /// </summary>
    public class CsvFileWriter : FileWriterBase, ICsvFileWriter
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ILogger<CsvFileWriter> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="logger"></param>
        public CsvFileWriter(IProcessedObservationRepository processedObservationRepository, 
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<CsvFileWriter> logger)
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
                var nrObservations = 0;
                var propertyFields =
                    ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.OutputFields);
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }

                var observationsFilePath = Path.Combine(temporaryZipExportFolderPath, "Observations.txt");
                await using var fileStream = File.Create(observationsFilePath);                
                using var csvFileHelper = new CsvFileHelper();
                csvFileHelper.InitializeWrite(fileStream, "\t");
                csvFileHelper.WriteRow(propertyFields.Select(pf => ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(pf, propertyLabelType)));
               
                var searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
                while (searchResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var processedObservations = searchResult.Records.ToArray();

                    // Resolve vocabulary values.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture);

                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        var flatObservation = new FlatObservation(observation);
                        
                        foreach (var propertyField in propertyFields)
                        {
                            var value = flatObservation.GetValue(propertyField);

                            var stringValue = value == null ? string.Empty : propertyField.DataTypeEnum switch
                            {
                                PropertyFieldDataType.Boolean => ((bool?)value)?.ToString(CultureInfo.InvariantCulture),
                                PropertyFieldDataType.DateTime => ((DateTime?) value)?.ToShortDateString(),
                                PropertyFieldDataType.Double => ((double) value).ToString(CultureInfo.InvariantCulture),
                                PropertyFieldDataType.Int32 => ((int)value).ToString(),
                                PropertyFieldDataType.Int64 => ((long)value).ToString(),
                                PropertyFieldDataType.TimeSpan => ((TimeSpan?)value)?.ToString("hh\\:mm"),
                                _ => (string)value
                            };

                            csvFileHelper.WriteField(stringValue);
                        }
                        csvFileHelper.NextRecord();
                    }

                    nrObservations += processedObservations.Length;
                    // Get next batch of observations.
                    searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter);
                }
                csvFileHelper.FinishWrite();

                if (gzip)
                {
                    await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                    var zipFilePath = _fileService.CompressFolder(exportPath, fileName);
                    return new FileExportResult
                    {
                        NrObservations = nrObservations,
                        FilePath = zipFilePath
                    };
                }
                else
                {
                    var destinationFilePath = Path.Combine(exportPath, $"{fileName}.csv");
                    File.Move(observationsFilePath, destinationFilePath);
                    return new FileExportResult
                    {
                        NrObservations = nrObservations,
                        FilePath = destinationFilePath
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
                sheet.Column(columnIndex).AutoFit();
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
