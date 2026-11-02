using Polly;
using Polly.Retry;

namespace SOS.Lib.Helpers;

public static class PollyHelper
{
    /// <summary>
    /// Get retry policy
    /// </summary>
    /// <param name="retries"></param>
    /// <param name="sleepMs"></param>
    /// <returns></returns>
    public static ResiliencePipeline GetRetryPolicy(int retries, int sleepMs) =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = retries,
                Delay = TimeSpan.FromMilliseconds(sleepMs),
                BackoffType = DelayBackoffType.Constant,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();
}
