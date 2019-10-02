using System.Threading.Tasks;

namespace SOS.Process.Factories.Interfaces
{
    /// <summary>
    /// Process base factory
    /// </summary>
    public interface IProcessFactory
    {
        /// <summary>
        /// Process sightings
        /// </summary>
        /// <returns></returns>
        Task<bool> ProcessAsync();
    }
}
