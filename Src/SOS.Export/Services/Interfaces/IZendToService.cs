using System.Threading.Tasks;
using SOS.Export.Models.ZendTo;
using SOS.Lib.Enums;

namespace SOS.Export.Services.Interfaces
{
    public interface IZendToService
    {
        /// <summary>
        /// Zend a file using ZendTo
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="description"></param>
        /// <param name="filePath"></param>
        /// <param name="exportFormat"></param>
        /// <param name="informRecipients"></param>
        /// <param name="informPasscode"></param>
        /// <param name="encryptFile"></param>
        /// <param name="encryptPassword"></param>
        /// <returns></returns>
        Task<ZendToResponse> SendFile(string emailAddress, string description, string filePath, ExportFormat exportFormat,
           bool informRecipients = true, bool informPasscode = true,  bool encryptFile = false, string encryptPassword = null);
    }
}