using System.IO;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    public interface ITaxonServiceProxy
    {
        /// <summary>
        /// Gets a checklist DwC-A file from a web service.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<Stream> GetDwcaFileAsync(string url);
    }
}