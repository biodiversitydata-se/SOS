using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Services.Interfaces
{
    public interface IDataCiteService
    {
        /// <summary>
        /// Create DOI
        /// </summary>
        /// <returns></returns>
        Task<bool> CreateDoiAsync();

        /// <summary>
        /// Get a metadata for a doi
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        Task<Models.DataCite.DOIResponse<Models.DataCite.DOIMetadata>> GetMetadataAsync(string prefix, string suffix);

        /// <summary>
        /// Search for DOI's
        /// </summary>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        Task<Models.DataCite.DOIResponse<IEnumerable<Models.DataCite.DOIMetadata>>> SearchMetadataAsync(string searchFor);
    }
}
