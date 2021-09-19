using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Jobs.Export
{
    /// <summary>
    ///     Interface for DOI export job
    /// </summary>
    public interface IExportAndSendJob
    {
        /// <summary>
        /// Run export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <param name="description"></param>
        /// <param name="exportFormat"></param>
        /// <param name="culture"></param>
        /// <param name="flatOut"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="outputFieldSet"></param>
        /// <returns></returns>
        [DisplayName("Export observations. Email={1}, Description={2}, ExportFormat={3}")]
        Task<bool> RunAsync(SearchFilter filter, 
            string email, 
            string description,
            ExportFormat exportFormat,
            string culture,
            bool flatOut,
            OutputFieldSet outputFieldSet,
            PropertyLabelType propertyLabelType,
            IJobCancellationToken cancellationToken);
    }
}