using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.Jobs.Export
{
    /// <summary>
    ///     Interface for DOI creation
    /// </summary>
    public interface ICreateDoiJob
    {
        /// <summary>
        /// Create a doi bys using provided filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="emailAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Create a DwC-A file using passed filter and give it a DOI")]
        [Queue("medium")]
        Task<bool> RunAsync(SearchFilter filter, string emailAddress, IJobCancellationToken cancellationToken);
    }
}