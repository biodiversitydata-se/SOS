using Polly;
using Polly.Retry;
using System;

namespace SOS.Lib.Helpers
{
    public static class PollyHelper
    {
        /// <summary>
        /// Get retry policy
        /// </summary>
        /// <param name="retries"></param>
        /// <param name="sleepMs"></param>
        /// <returns></returns>
        public static AsyncRetryPolicy GetRetryPolicy(int retries, int sleepMs) => Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount: retries, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(sleepMs));
    }
}
