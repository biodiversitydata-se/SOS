using System.Threading.Tasks;

namespace SOS.Process.Managers.Interfaces
{
    public interface IProcessManager
    {
        /// <summary>
        /// Release Thread
        /// </summary>
        /// <returns></returns>
        int Release();

        /// <summary>
        /// Wait for thread to finish
        /// </summary>
        /// <returns></returns>
        Task WaitAsync();
    }
}
