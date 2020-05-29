using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    public interface ITaxonServiceProxy
    {
        /// <summary>
        ///     Gets a checklist DwC-A file from a web service.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<Stream> GetDwcaFileAsync(string url);

        /// <summary>
        ///     Gets basic information about a taxon
        /// </summary>
        /// <param name="url"></param>
        /// <param name="taxonIds"></param>
        /// <returns></returns>
        Task<string> GetTaxonAsync(string url, IEnumerable<int> taxonIds);
    }
}