using Hangfire;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import;

public interface IAreasHarvestJob
{
    /// <summary>
    ///     Run geo harvest
    /// </summary>
    /// <returns></returns>
    [JobDisplayName("Harvest areas")]
    [Queue("high")]
    Task<bool> RunAsync();
}