using Nest;
using System;

namespace SOS.Lib.Extensions
{
    public static class ElasticSearchExtensions
    {
        private static void ThrowException(Exception originalException, string debugInformation)
        {
            if (originalException != null)
            {
                if (originalException.Message?.Contains("The request was canceled due to the configured HttpClient.Timeout", StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    throw new TimeoutException(originalException.Message);
                }

                throw originalException;
            }

            throw new InvalidOperationException(debugInformation);
        }

        public static void ThrowIfInvalid(this CountResponse response) 
        {
            if (!response.IsValid)
            {
                ThrowException(response.OriginalException, response.DebugInformation);
            }
        }

        public static void ThrowIfInvalid<T>(this ISearchResponse<T> response) where T : class
        {
            if (!response.IsValid)
            {
                ThrowException(response.OriginalException, response.DebugInformation);
            }
        }
    }

}
