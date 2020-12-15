using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.DataCite;

namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    /// DataCite service interface
    /// </summary>
    public interface IDataCiteService
    {
        /// <summary>
        /// Create DOI draft
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<DOIMetadata> CreateDoiDraftAsync(DOIMetadata data);

        /// <summary>
        /// Get batch of DOI's
        /// </summary>
        /// <param name="take"></param>
        /// <param name="page"></param>
        /// <param name="orderBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<DOI<IEnumerable<DOIMetadata>>> GetBatchAsync(int take, int page, string orderBy, SearchSortOrder sortOrder);

        /// <summary>
        /// Get a metadata for a doi
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        Task<DOIMetadata> GetMetadataAsync(string prefix, string suffix);

        /// <summary>
        /// Publish a DOI and make it findable
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> PublishDoiAsync(DOIMetadata data);

        /// <summary>
        /// Search for DOI's
        /// </summary>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        Task<DOI<IEnumerable<DOIMetadata>>> SearchMetadataAsync(string searchFor);
    }
}
