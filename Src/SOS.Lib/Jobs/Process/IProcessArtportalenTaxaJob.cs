using Hangfire;

namespace SOS.Lib.Jobs.Process;

public interface IProcessArtportalenTaxaJob
{
    /// <summary>
    ///     Reads Artportalen taxonomy from api, adds it to processed database.
    /// </summary>
    /// <returns></returns>
    [JobDisplayName("Process Artportalen taxa")]
    [Queue("high")]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    Task<bool> RunAsync();
}
