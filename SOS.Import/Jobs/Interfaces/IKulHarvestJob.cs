using System.Threading.Tasks;
using Hangfire;

namespace SOS.Import.Jobs.Interfaces
{
    public interface IKulHarvestJob
    {
        /// <summary>
        /// Run KUL harvest.
        /// </summary>
        /// <returns></returns>
        Task<bool> Run(IJobCancellationToken cancellationToken);
    }
}
