using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessAreasJob
    {
        /// <summary>
        ///     Copy areas from verbatim db to process db.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}