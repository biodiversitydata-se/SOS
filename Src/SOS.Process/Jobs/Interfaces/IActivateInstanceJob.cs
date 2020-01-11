using System.Threading.Tasks;

namespace SOS.Process.Jobs.Interfaces
{
    public interface IActivateInstanceJob
    {
        /// <summary>
        /// Activate passed instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> RunAsync(byte instance);
    }
}
