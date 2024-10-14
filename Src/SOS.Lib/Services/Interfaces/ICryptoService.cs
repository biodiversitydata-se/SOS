using System.Threading.Tasks;

namespace SOS.Lib.Services.Interfaces
{
    public interface ICryptoService
    {
        /// <summary>
        /// Decrypt string
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        Task<string> DecryptAsync(string encrypted);

        /// <summary>
        /// Encrypt string
        /// </summary>
        /// <param name="clearText"></param>
        /// <returns></returns>
        Task<string> EncryptAsync(string clearText);
    }
}
