using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Managers
{
    public class InvalidObservationsManager : IInvalidObservationsManager
    {
        private readonly IInvalidObservationRepository _invalidObservationRepository;
        private readonly IReportManager _reportManager;
        private readonly ILogger<InvalidObservationsManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="invalidObservationRepository"></param>
        /// <param name="reportManager"></param>
        /// <param name="logger"></param>
        public InvalidObservationsManager(
            IInvalidObservationRepository invalidObservationRepository,
            IReportManager reportManager,
            ILogger<InvalidObservationsManager> logger)
        {
            _invalidObservationRepository = invalidObservationRepository ?? throw new ArgumentNullException(nameof(invalidObservationRepository));
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateExcelFileReportAsync(string reportId, int dataProviderId, string createdBy)
        {
            try
            {
                var excelWriter = new InvalidObservationsExcelWriter(_invalidObservationRepository);
                var excelFile = await excelWriter.CreateExcelFile(dataProviderId.ToString());
                var report = new Report(reportId)
                {
                    Type = ReportType.InvalidObservations,
                    Name = "Invalid observation",
                    FileExtension = "xlsx",
                    CreatedBy = createdBy ?? "",
                    FileSizeInKb = excelFile.Length / 1024
                };

                await _reportManager.AddReportAsync(report, excelFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Create Invalid observations Excel file failed.");
                return false;
            }

            return true;
        }

        public class InvalidObservationsExcelWriter
        {
            private string _datasetId;
            private readonly IInvalidObservationRepository _invalidObservationRepository;
            private const int MaxNumberOfExcelRows = 1000000; // The maximun number of rows in a excel sheet.
            private const int OccurrenceIdColumnIndex = 1;
            private const int DatasetIdColumnIndex = 2;
            private const int DatasetNameColumnIndex = 3;
            private const int ModifiedDateColumnIndex = 4;
            private const int TaxonNotFoundColumnIndex = 5;
            private const int MissingMandatoryFieldColumnIndex = 6;
            private const int LocationOutsideOfSwedenColumnIndex = 7;
            private const int ValueOutOfRangeColumnIndex = 8;
            private const int LogicErrorColumnIndex = 9;

            public InvalidObservationsExcelWriter(IInvalidObservationRepository invalidObservationRepository)
            {
                _invalidObservationRepository = invalidObservationRepository ?? throw new ArgumentNullException(nameof(invalidObservationRepository));
            }

            public async Task<byte[]> CreateExcelFile(string datasetId)
            {
                _datasetId = datasetId;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Invalid observations");
                AddHeaders(sheet);
                FormatColumns(sheet);
                await AddData(sheet);

                byte[] excelBytes = await package.GetAsByteArrayAsync();
                return excelBytes;
            }

            private async Task AddData(ExcelWorksheet worksheet)
            {
                var rowIndex = 2;

                using var cursor = await _invalidObservationRepository.GetAllByCursorAsync();
                while (await cursor.MoveNextAsync())
                {
                    foreach (var row in cursor.Current)
                    {
                        if (row.DatasetID != _datasetId) continue; // todo - move filter to _invalidObservationRepository.GetAllByCursorAsync() call.

                        worksheet.Cells[rowIndex, OccurrenceIdColumnIndex].Value = row.OccurrenceID;
                        worksheet.Cells[rowIndex, DatasetIdColumnIndex].Value = row.DatasetID;
                        worksheet.Cells[rowIndex, DatasetNameColumnIndex].Value = row.DatasetName;
                        worksheet.Cells[rowIndex, ModifiedDateColumnIndex].Value = row.ModifiedDate;

                        if (row.Defects != null)
                        {
                            foreach (var defect in row.Defects.GroupBy(m => m.DefectType).Select(m => m.First()))
                            {
                                switch (defect.DefectType)
                                {
                                    case Models.Processed.Validation.ObservationDefect.ObservationDefectType.TaxonNotFound:
                                        worksheet.Cells[rowIndex, TaxonNotFoundColumnIndex].Value = defect.Information;
                                        break;
                                    case Models.Processed.Validation.ObservationDefect.ObservationDefectType.MissingMandatoryField:
                                        worksheet.Cells[rowIndex, MissingMandatoryFieldColumnIndex].Value = defect.Information;
                                        break;
                                    case Models.Processed.Validation.ObservationDefect.ObservationDefectType.LocationOutsideOfSweden:
                                        worksheet.Cells[rowIndex, LocationOutsideOfSwedenColumnIndex].Value = defect.Information;
                                        break;
                                    case Models.Processed.Validation.ObservationDefect.ObservationDefectType.ValueOutOfRange:
                                        worksheet.Cells[rowIndex, ValueOutOfRangeColumnIndex].Value = defect.Information;
                                        break;
                                    case Models.Processed.Validation.ObservationDefect.ObservationDefectType.LogicError:
                                        worksheet.Cells[rowIndex, LogicErrorColumnIndex].Value = defect.Information;
                                        break;
                                }
                            }
                        }
                        
                        rowIndex++;
                    }
                }
            }

            private void FormatColumns(ExcelWorksheet worksheet)
            {
                const int firstDataRow = 2;
                var lastDataRow = MaxNumberOfExcelRows + 1;

                worksheet.Cells[firstDataRow, ModifiedDateColumnIndex, lastDataRow, ModifiedDateColumnIndex].Style.Numberformat.Format = "yyyy-MM-dd";                
            }

            private void AddHeaders(ExcelWorksheet worksheet)
            {
                worksheet.Cells[1, OccurrenceIdColumnIndex].Value = "OccurrenceId";
                worksheet.Cells[1, DatasetIdColumnIndex].Value = "DatasetId";
                worksheet.Cells[1, DatasetNameColumnIndex].Value = "DatasetName";
                worksheet.Cells[1, ModifiedDateColumnIndex].Value = "ModifiedDate";
                worksheet.Cells[1, TaxonNotFoundColumnIndex].Value = "TaxonNotFound";
                worksheet.Cells[1, MissingMandatoryFieldColumnIndex].Value = "MissingMandatoryField";
                worksheet.Cells[1, LocationOutsideOfSwedenColumnIndex].Value = "LocationOutsideSweden";
                worksheet.Cells[1, ValueOutOfRangeColumnIndex].Value = "ValueOutOfRange";
                worksheet.Cells[1, LogicErrorColumnIndex].Value = "LogicError";                

                // Format style by columns in first row
                using (var range = worksheet.Cells[1, 1, 1, LogicErrorColumnIndex])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Font.Color.SetColor(Color.FromArgb(255, 255, 255));
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                }
            }
        }
    }
}