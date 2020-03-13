using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessTaxaJob
    {
        /// <summary>
        /// Read taxonomy from verbatim database, do some conversions and adds it to processed database.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}
