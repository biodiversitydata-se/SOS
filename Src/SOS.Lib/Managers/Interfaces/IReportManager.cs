using CSharpFunctionalExtensions;
using SOS.Lib.Models.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    ///     Report manager interface.
    /// </summary>
    public interface IReportManager
    {
        Task AddReportAsync(Report report, byte[] file);
        Task<Result<ReportFile>> GetReportFileAsync(string reportId);
        Task<Report> GetReportAsync(string reportId);
        Task<IEnumerable<Report>> GetAllReportsAsync();
        Task<Result<int>> DeleteOldReportsAndFilesAsync(TimeSpan timeSpan);
        Task DeleteReportAsync(string reportId);
    }
}