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
        /// Returns a list of data provider datasets (DwC-A) available for download. A file is usually created once a day for each dataset.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetDatasetsList();

        /// <summary>
        /// Get all exports for a user
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetMyExports();

        /// <summary>
        /// Get export by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IActionResult> GetMyExport(string id);

        /// <summary>
        ///  Download Csv export file. The limit is 25 000 observations. If you need to download more observations, use the OrderCsv endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        Task<IActionResult> DownloadCsv(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter,
            OutputFieldSet outputFieldSet,
            bool validateSearchFilter = false,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            string cultureCode = "sv-SE",
            bool gzip = true,
            bool sensitiveObservations = false);

        /// <summary>
        /// Download DwC export file. The limit is 25 000 observations. If you need to download more observations, use the OrderDwC endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        Task<IActionResult> DownloadDwC(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter,
            bool validateSearchFilter = false,
            bool sensitiveObservations = false);

        /// <summary>
        ///  Download Excel export file. The limit is 25 000 observations. If you need to download more observations, use the OrderExcel endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        Task<IActionResult> DownloadExcel(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter,
            OutputFieldSet outputFieldSet,
            bool validateSearchFilter = false,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName, 
            string cultureCode = "sv-SE",
            bool gzip = true,
            bool sensitiveObservations = false);

        /// <summary>
        /// Download GeoJson export file. The limit is 25 000 observations. If you need to download more observations, use the OrderGeoJson endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The label type to use if flat=false.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="flat">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        Task<IActionResult> DownloadGeoJson(
            int? roleId,
            string authorizationApplicationIdentifier, 
            SearchFilterDto filter,
            OutputFieldSet outputFieldSet,
            bool validateSearchFilter = false,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            string cultureCode = "sv-SE",
            bool flat = true,
            bool excludeNullValues = true,
            bool gzip = true,
            bool sensitiveObservations = false);

        /// <summary>
        /// Starts the process of creating a Csv file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        Task<IActionResult> OrderCsv(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter,
            string description,
            OutputFieldSet outputFieldSet,
            bool validateSearchFilter = false,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            bool sensitiveObservations = false,
            bool sendMailFromZendTo = true,
            string encryptPassword = "",
            string confirmEncryptPassword = "",
            string cultureCode = "sv-SE");

        /// <summary>
        /// Starts the process of creating a DwC-A file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <returns></returns>
        Task<IActionResult> OrderDwC(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter, 
            string description,
            bool validateSearchFilter = false,
            bool sensitiveObservations = false,
            bool sendMailFromZendTo = true,
            string encryptPassword = "",
            string confirmEncryptPassword = "");

        /// <summary>
        /// Starts the process of creating a Excel file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        Task<IActionResult> OrderExcel(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter, 
            string description,
            OutputFieldSet outputFieldSet,
            bool validateSearchFilter = false,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            bool sensitiveObservations = false,
            bool sendMailFromZendTo = true,
            string encryptPassword = "",
            string confirmEncryptPassword = "",
            string cultureCode = "sv-SE");

        /// <summary>
        /// Starts the process of creating a GeoJSON file with observations based on the provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="description">A description of your download. Will be displayed in the email.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="propertyLabelType">The label type to use if flat=false.</param>
        /// <param name="flat">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values.</param>
        /// <param name="cultureCode">The culture code used for translation vocabulary values.</param>

        /// <returns></returns>
        Task<IActionResult> OrderGeoJson(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter,
            string description,
            OutputFieldSet outputFieldSet,
            bool validateSearchFilter = false,
            PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            bool sensitiveObservations = false,
            bool sendMailFromZendTo = true,
            string encryptPassword = "",
            string confirmEncryptPassword = "",
            bool flat = true,
            bool excludeNullValues = true,
            string cultureCode = "sv-SE");
    }
}