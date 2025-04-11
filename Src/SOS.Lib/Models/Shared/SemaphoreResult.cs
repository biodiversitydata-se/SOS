using System;
using System.Threading;

namespace SOS.Lib.Models.Shared;
public class SemaphoreResult
{
    public SemaphoreSlim? Semaphore { get; set; }
    public string SemaphoreStatus { get; set; } = string.Empty;
    public TimeSpan? SemaphoreWaitTime { get; set; }
}
