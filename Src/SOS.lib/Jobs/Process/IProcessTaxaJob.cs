using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessTaxaJob
    {
        /// <summary>
        ///     Read taxonomy from verbatim database, do some conversions and adds it to processed database.
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("Process taxa")]
        [Queue("high")]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task<bool> RunAsync();
    }
}