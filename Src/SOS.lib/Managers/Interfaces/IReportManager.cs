using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonTree;

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
    }
}