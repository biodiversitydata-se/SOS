using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Process
{
    public interface ICopyAreasJob
    {
        /// <summary>
        /// Copy areas from verbatim db to process db.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}