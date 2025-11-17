using Hangfire;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import;

public interface IProjectsHarvestJob
{
    /// <summary>
    ///     Run harvest projects vocabulary job.
    /// </summary>
    /// <returns></returns>
    [JobDisplayName("Harvest projects from Artportalen db")]
    [Queue("high")]
    Task<bool> RunHarvestProjectsAsync();
}