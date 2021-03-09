using System.ComponentModel;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IProjectsHarvestJob
    {
        /// <summary>
        ///     Run harvest projects vocabulary job.
        /// </summary>
        /// <returns></returns>
        [DisplayName("Harvest projects from Artportalen db")]
        Task<bool> RunHarvestProjectsAsync();
    }
}