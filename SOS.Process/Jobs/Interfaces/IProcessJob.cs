using System.Threading.Tasks;

namespace SOS.Process.Jobs.Interfaces
{
    public interface IProcessJob
    {
        /// <summary>
        /// Process data
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        Task<bool> Run(int sources);
    }
}
