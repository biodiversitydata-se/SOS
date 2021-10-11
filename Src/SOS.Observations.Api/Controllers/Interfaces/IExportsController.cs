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
        ///  Download Csv export file. The limit is 25 000 observations. If you need to download more observations, use the OrderCsv endpoint.
        /// </summary>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">The observation property field set.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        Task<IActionResult> DownloadCsv(
            [FromBody] ExportFilterDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.ShortPropertyName,
            [FromQuery] string cultureCode = "sv-SE");

        /// <summary>
        /// Download DwC export file. The limit is 25 000 observations. If you need to download more observations, use the OrderDwC endpoint.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> DownloadDwC(ExportFilterDto filter);

        /// <summary>
        ///  Download Excel export file. The limit is 25 000 observations. If you need to download more observations, use the OrderExcel endpoint.
        /// </summary>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">The observation property field set.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        Task<IActionResult> DownloadExcel(ExportFilterDto filter,
            OutputFieldSet outputFieldSet = OutputFieldSet.Minimum, 
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName, 
            string cultureCode = "sv-SE");

        /// <summary>
        /// Download GeoJson export file. The limit is 25 000 observations. If you need to download more observations, use the OrderGeoJson endpoint.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="outputFieldSet">The observation property field set.</param>
        /// <param name="propertyLabelType">The label type to use if flatOut=false.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="flatOut">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values.</param>
        /// <returns></returns>
        Task<IActionResult> DownloadGeoJson(ExportFilterDto filter,
            OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            string cultureCode = "sv-SE",
            bool flatOut = true,
            bool excludeNullValues = true);

        /// <summary>
        /// Starts the process of creating a Csv file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="description"></param>
        /// <param name="outputFieldSet"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        Task<IActionResult> OrderCsv(ExportFilterDto filter,
            string description,
            OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            PropertyLabelType propertyLabelType = PropertyLabelType.ShortPropertyName,
            string cultureCode = "sv-SE");

        /// <summary>
        /// Starts the process of creating a DwC-A file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <returns></returns>
        Task<IActionResult> OrderDwC(ExportFilterDto filter, string description);

        /// <summary>
        /// Starts the process of creating a Excel file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="outputFieldSet">The observation property field set.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        Task<IActionResult> OrderExcel(ExportFilterDto filter, 
            string description,
            OutputFieldSet outputFieldSet = OutputFieldSet.Minimum, 
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName, 
            string cultureCode = "sv-SE");

        /// <summary>
        /// Starts the process of creating a GeoJSON file with observations based on the provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="description">A description of your download. Will be displayed in the email.</param>
        /// <param name="outputFieldSet">The observation property field set.</param>
        /// <param name="propertyLabelType">The label type to use if flatOut=false.</param>
        /// <param name="cultureCode">The culture code used for translation vocabulary values.</param>
        /// <param name="flatOut">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values.</param>
        /// <returns></returns>
        Task<IActionResult> OrderGeoJson(ExportFilterDto filter,
            string description,
            OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            string cultureCode = "sv-SE",
            bool flatOut = true,
            bool excludeNullValues = true);
    }
}