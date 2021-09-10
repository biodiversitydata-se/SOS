using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Export job controller
    /// </summary>
    public interface IExportsController
    {
        /// <summary>
        /// Returns a list of datasets available for download. A DwC-A file is usually created once a day for each dataset.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetDatasetsList();

        /// <summary>
        /// Download DwC export file
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> DownloadDwC(ExportFilterDto filter);

        /// <summary>
        ///  Download Excel export file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputFieldSet"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        Task<IActionResult> DownloadExcel(ExportFilterDto filter,
            OutputFieldSet outputFieldSet, string cultureCode);

        /// <summary>
        /// Download GeoJson export file 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputFieldSet"></param>
        /// <param name="cultureCode"></param>
        /// <param name="flatOut"></param>
        /// <returns></returns>
        Task<IActionResult> DownloadGeoJson(ExportFilterDto filter,
           OutputFieldSet outputFieldSet, string cultureCode, bool flatOut);

        /// <summary>
        /// Starts the process of creating a DwC-A file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <returns></returns>
        Task<IActionResult> OrderDwC(ExportFilterDto filter, string description);

        /// <summary>
        ///  Starts the process of creating a Excel file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="description"></param>
        /// <param name="outputFieldSet"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        Task<IActionResult> OrderExcel(ExportFilterDto filter, string description,
            OutputFieldSet outputFieldSet, string cultureCode);

        /// <summary>
        /// Starts the process of creating a GeoJson file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="description"></param>
        /// <param name="outputFieldSet"></param>
        /// <param name="cultureCode"></param>
        /// <param name="flatOut"></param>
        /// <returns></returns>
        Task<IActionResult> OrderGeoJson(ExportFilterDto filter, string description,
            OutputFieldSet outputFieldSet, string cultureCode, bool flatOut);
    }
}