using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Managers
{
    /// <summary>
    ///     Report manager
    /// </summary>
    public class ReportManager : IReportManager
    {
        private readonly ILogger<ReportManager> _logger;
        private readonly IReportRepository _reportRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reportRepository"></param>
        /// <param name="logger"></param>
        public ReportManager(
            IReportRepository reportRepository,
            ILogger<ReportManager> logger)
        {
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddReportAsync(Report report, byte[] file)
        {
            await _reportRepository.AddAsync(report);
            await _reportRepository.StoreFileAsync(report.Id, file);
        }

        public async Task<Result<ReportFile>> GetReportFileAsync(string reportId)
        {
            var report = await GetReportAsync(reportId);
            if (report == null)
            {
                return Result.Failure<ReportFile>($"Report with reportId \"{reportId}\" not found");
            }

            var file = await _reportRepository.GetFileAsync(reportId);
            if (file == null || file.Length == 0)
            {
                return Result.Failure<ReportFile>($"File for reportId \"{reportId}\" not found");
            }

            var filename = FilenameHelper.CreateFilenameWithDate($"{report.Type}-{report.Name}", report.FileExtension, report.CreatedDate);
            var reportFileDownload = new ReportFile
            {
                //Filename = $"{report.Type}-{report.Name}.{report.FileExtension}", File = file, ContentType = "application/zip"
                Filename = filename,
                File = file,
                ContentType = "application/zip"
            };

            return reportFileDownload;
        }

        public async Task<Report> GetReportAsync(string reportId)
        {
            return await _reportRepository.GetAsync(reportId);
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _reportRepository.GetAllAsync();
        }

        public async Task<Result<int>> DeleteOldReportsAndFilesAsync(TimeSpan timeSpan)
        {
            DateTime date = DateTime.UtcNow - timeSpan;
            List<string> deleteReportIds = new List<string>();
            var allReports = await GetAllReportsAsync();
            foreach (var reportFile in allReports)
            {
                if (reportFile.CreatedDate < date)
                {
                    deleteReportIds.Add(reportFile.Id);
                }
            }

            if (deleteReportIds.Count > 0)
            {
                await _reportRepository.DeleteManyAsync(deleteReportIds);
                await _reportRepository.DeleteFilesAsync(deleteReportIds);
            }

            return Result.Success(deleteReportIds.Count);
        }
    }
}