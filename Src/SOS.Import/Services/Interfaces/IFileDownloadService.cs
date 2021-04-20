using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Import.Services.Interfaces
{
    public interface IFileDownloadService
    {
        /// <summary>
        /// Download xml file
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<XDocument> GetXmlFileAsync(string url);

        /// <summary>
        /// Download file and store it
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="acceptHeaderContentType"></param>
        /// <returns></returns>
        Task<bool> GetFileAndStoreAsync(string url, string path, string acceptHeaderContentType = null);
    }
}