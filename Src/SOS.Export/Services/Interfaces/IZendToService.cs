using System.Threading.Tasks;
using SOS.Lib.Enums;

namespace SOS.Export.Services.Interfaces
{
    public interface IZendToService
    {
        /// <summary>
        ///  Zend a file using ZendTo
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="description"></param>
        /// <param name="filePath"></param>
        /// <param name="exportFormat"></param>
        /// <returns></returns>
        Task<bool> SendFile(string emailAddress, string description, string filePath, ExportFormat exportFormat);
    }
}