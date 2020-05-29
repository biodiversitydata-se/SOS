using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IHarvestJob
    {
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}