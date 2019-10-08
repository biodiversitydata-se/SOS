using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Process.Models.Processed;

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
        /// <param name="taxa"></param>
        /// <returns></returns>
        Task<bool> ProcessAsync(IDictionary<string, DarwinCoreTaxon> taxa);
    }
}
