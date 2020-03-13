using System.Threading.Tasks;

namespace SOS.Export.Services.Interfaces
{
    public interface IZendToService
    {
        /// <summary>
        /// Zend a file using ZendTo
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<bool> SendFile(string filePath);
    }
}
