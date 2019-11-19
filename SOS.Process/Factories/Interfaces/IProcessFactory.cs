using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.DarwinCore;

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
        /// <param name="databaseName"></param>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ProcessAsync(string databaseName, IDictionary<int, DarwinCoreTaxon> taxa, IJobCancellationToken cancellationToken);

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="databaseName"></param>
        void Initialize(string databaseName);
    }
}
